using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.AI;
using Somnia.Game.Services.Audio;
using Somnia.Game.Services.Camera;
using Somnia.Game.Services.Combat;
using Somnia.Game.Services.Economy;
using Somnia.Game.Services.Npc;
using Somnia.Game.Services.Particles;
using Somnia.Game.Services.Projectiles;
using Somnia.Game.Services.Waves;
using Somnia.Game.Services.World;
using Somnia.Game.Session;

namespace Somnia.Game.Controllers;

/// <summary>Оркестрация одного кадра игры: AI, снаряды, лут, гейты, трясёт камеру.</summary>
public sealed class GameplayOrchestrator
{
    /// <summary>Задержка перед автопереходом арены после уничтожения всех врагов.</summary>
    public const float WipeoutAdvanceDelay = 0.6f;

    /// <summary>Полный таймер арены в секундах. По истечении — овертайм и подкрепления.</summary>
    public const float ArenaTimerMaxSeconds = 90f;

    /// <summary>Интервал между волнами овертайм-чарджеров (в секундах).</summary>
    public const float OvertimeReinforcementInterval = 6f;

    /// <summary>Урон по игроку за секунду овертайма (тикает каждый кадр).</summary>
    public const float OvertimeDamagePerSecond = 4f;

    /// <summary>Волны подкреплений на босс-арене (только пока не истёк таймер арены).</summary>
    public const float BossReinforcementIntervalSeconds = 19f;

    public const int MaxBossReinforcementWaves = 8;

    private readonly ArenaLayoutGenerator _arenaGen = new();
    private readonly LineOfSightService _los = new();
    private readonly IPlayerCombatService _combat;
    private readonly PlayerProjectileSimulator _playerProjSim;
    private readonly EnemyProjectileSimulator _enemyProjSim = new();
    private readonly ResourceDropOrchestrator _dropOrchestrator = new();
    private readonly DeathLootSpawnService _deathLoot = new();
    private readonly NpcCarryInteractionService _npcCarry = new();
    private readonly EnemyAiService _enemyAi = new();
    private readonly CameraShakeService _cameraShake;
    private readonly FloorEffectService _floorFx;
    private readonly WallSparkleEmitter _wallSparkler;
    private float _overtimeReinforcementClock;
    private AudioController? _audio;

    public PlayerInputController PlayerInput { get; }

    public void SetAudio(AudioController? audio) => _audio = audio;

    public GameplayOrchestrator(PlayerModel player, IPlayerCombatService? combatOverride = null,
        CameraShakeService? shakeOverride = null)
    {
        _combat = combatOverride ?? new PlayerCombatService(_los);
        _playerProjSim = new PlayerProjectileSimulator(_los);
        _cameraShake = shakeOverride ?? new CameraShakeService();
        _floorFx = new FloorEffectService();
        _wallSparkler = new WallSparkleEmitter();
        PlayerInput = new PlayerInputController(player, _combat);
    }

    public void RestartGame(GameplaySessionState s, int width, int height, Random? rng = null)
    {
        rng ??= new Random();

        s.UiState = GameplayPhase.Playing;
        s.PlayArea = new Rectangle(0, 0, width, height);
        s.Waves = new WaveManager();
        s.SecretMeatVictory = false;
        s.ArenaLayoutSeed = rng.Next();

        ResetBodiesAndLists(s);
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
        PlaceNpcAwayFromEnemies(s, rng);
    }

    /// <summary>
    /// Отладка: мгновенно загрузить босс-арену (те же шаги, что при переходе по волнам).
    /// Вызывается из скрытого шортката в <see cref="Game1"/>.
    /// </summary>
    public void DebugJumpToBossArena(GameplaySessionState s, Random? rng = null)
    {
        rng ??= new Random();
        s.Waves.ExitSecretMeatGrinderMode();
        s.SecretMeatVictory = false;
        s.Waves.DebugJumpToBossArena();
        s.ArenaLayoutSeed = rng.Next();
        ResetBodiesAndLists(s);
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
        PlaceNpcAwayFromEnemies(s, rng);
        s.WaveClearTimer = 0f;
    }

    /// <summary>Секретная «мясорубка»: Ctrl+Shift+M из игры. Не влияет на индекс кампании.</summary>
    public void DebugEnterSecretMeatGrinder(GameplaySessionState s, Random? rng = null)
    {
        rng ??= new Random();
        s.Waves.EnterSecretMeatGrinderMode();
        s.SecretMeatVictory = false;
        s.ArenaLayoutSeed = rng.Next();
        ResetBodiesAndLists(s);
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
        PlaceNpcAwayFromEnemies(s, rng);
        s.WaveClearTimer = 0f;
    }

    public bool TryAdvanceArena(GameplaySessionState s, Random? rng = null)
    {
        rng ??= new Random();

        s.Waves.AdvanceArena();
        if (s.Waves.AllArenasCleared)
        {
            s.UiState = GameplayPhase.GameOver;
            return false;
        }

        s.ArenaLayoutSeed = rng.Next();
        ResetBodiesAndLists(s);
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
        PlaceNpcAwayFromEnemies(s, rng);
        return true;
    }

    /// <summary>
    /// Ставит NPC рядом с игроком, но не вплотную (см. <see cref="NpcMinPlayerDistance"/>..<see cref="NpcMaxPlayerDistance"/>),
    /// и подальше от врагов (≥ <see cref="NpcSafeRadius"/>). Пробует случайные точки;
    /// если все плохие — выбирает «наименее опасную».
    /// </summary>
    private static void PlaceNpcAwayFromEnemies(GameplaySessionState s, Random rng)
    {
        Vector2 best = s.Npc.Position;
        float bestScore = float.NegativeInfinity;

        for (var attempt = 0; attempt < 48; attempt++)
        {
            // Берём точку «вокруг игрока» в кольце [NpcMinPlayerDistance, NpcMaxPlayerDistance].
            var angle = (float)(rng.NextDouble() * System.Math.PI * 2);
            var radius = NpcMinPlayerDistance +
                         (float)rng.NextDouble() * (NpcMaxPlayerDistance - NpcMinPlayerDistance);
            var cand = s.Player.Position + new Vector2(
                (float)System.Math.Cos(angle) * radius,
                (float)System.Math.Sin(angle) * radius);

            // Не вылезаем за арену.
            cand.X = MathHelper.Clamp(cand.X, s.PlayArea.Left + 120f, s.PlayArea.Right - 120f);
            cand.Y = MathHelper.Clamp(cand.Y, s.PlayArea.Top + 120f, s.PlayArea.Bottom - 120f);

            // Минимум до ближайшего живого врага.
            var nearestEnemy = float.MaxValue;
            foreach (var e in s.Enemies)
            {
                if (e.IsDead) continue;
                var d = Vector2.Distance(cand, e.Position);
                if (d < nearestEnemy) nearestEnemy = d;
            }

            // Не вплотную к стене.
            var nearestWall = float.MaxValue;
            foreach (var w in s.Walls)
            {
                var d = Vector2.Distance(cand, w.Center) - w.Radius;
                if (d < nearestWall) nearestWall = d;
            }

            if (nearestEnemy >= NpcSafeRadius && nearestWall > 32f)
            {
                s.Npc.Position = cand;
                return;
            }

            // Если ни одна попытка не идеальна — сохраним «лучшую» по сумме дистанций.
            var score = nearestEnemy + nearestWall * 0.5f;
            if (score > bestScore)
            {
                bestScore = score;
                best = cand;
            }
        }

        s.Npc.Position = best;
    }

    /// <summary>Минимальное расстояние от NPC до любого живого врага при спавне.</summary>
    public const float NpcSafeRadius = 280f;

    /// <summary>NPC спавнится в кольце вокруг игрока: ближе — некуда (он сразу залезет на голову),
    /// а дальше — игрок не видит куда идти.</summary>
    public const float NpcMinPlayerDistance = 180f;

    public const float NpcMaxPlayerDistance = 360f;

    private void ResetBodiesAndLists(GameplaySessionState s)
    {
        s.Player.ResetForRun();
        s.Npc.ResetForRun();

        s.Player.Position = new Vector2(250f, s.PlayArea.Height / 2f);
        s.Player.SetState(PlayerState.Free);

        // NPC спавнится случайно по seed арены — где-то в середине-дали, не у самой стартовой/гейта,
        // чтобы за ним приходилось идти, но позиция всегда разная и читаемая для зачистки.
        var rnd = new Random(s.ArenaLayoutSeed ^ 0x52766f1d);
        var minX = s.PlayArea.Width * 0.35f;
        var maxX = s.PlayArea.Width * 0.78f;
        var minY = s.PlayArea.Height * 0.18f;
        var maxY = s.PlayArea.Height * 0.82f;
        s.Npc.Position = new Vector2(
            (float)(minX + rnd.NextDouble() * (maxX - minX)),
            (float)(minY + rnd.NextDouble() * (maxY - minY)));

        s.PlayerProjectiles.Clear();
        s.EnemyProjectiles.Clear();
        s.Drops.Clear();
        s.FloatingTexts.Clear();
        s.FloorSplatters.Clear();
        s.WallSparkles.Clear();

        s.Gates.Clear();
        if (!s.Waves.IsBossArena && !s.Waves.IsSecretMeatGrinder)
        {
            // Обычные арены: гейт справа, требует и доставки NPC, и зачистки большинства врагов.
            s.Gates.Add(new GateModel(new Vector2(s.PlayArea.Width - 200f, s.PlayArea.Height / 2f)));
        }
        // На босс-арене гейтов нет — победа только через смерть босса (срабатывает wipeout).

        s.ArenaIntroGraceSeconds = s.Waves.IsBossArena
            ? 3.35f
            : s.Waves.IsSecretMeatGrinder ? 2.25f : 0f;

        s.BossReinforcementTimer = 0f;
        s.BossReinforcementWavesDone = 0;

        s.Camera.ShakeTrauma = 0f;
        s.Camera.ShakeOffset = Vector2.Zero;
        s.WaveClearTimer = 0f;
        s.ArenaTimer = s.Waves.IsBossArena || s.Waves.IsSecretMeatGrinder
            ? ArenaTimerMaxSeconds * 2.5f   // боссфайту нужно больше времени
            : ArenaTimerMaxSeconds;
        s.OvertimeElapsed = 0f;
        _overtimeReinforcementClock = 0f;
    }

    private void RebuildArena(GameplaySessionState s)
    {
        var layout = s.Waves.IsSecretMeatGrinder
            ? _arenaGen.Generate(s.PlayArea, s.ArenaLayoutSeed ^ unchecked((int)0xc001d00d), anomalyTargetCount: 20)
            : s.Waves.IsBossArena
                ? BossArenaLayout.Build(s.PlayArea, s.ArenaLayoutSeed)
                : _arenaGen.Generate(s.PlayArea, s.ArenaLayoutSeed);
        s.Zones.Clear();
        s.Zones.AddRange(layout.Zones);
        s.Walls.Clear();
        s.Walls.AddRange(layout.Walls);
        s.BossZoneShiftClock = 0f;
    }

    private static void SpawnEnemiesForCurrentWave(GameplaySessionState s)
    {
        s.Enemies.Clear();
        s.Enemies.AddRange(s.Waves.SpawnCurrentWave(s.PlayArea.Width, s.PlayArea.Height));
        s.TotalEnemiesInArena = s.Enemies.Count;
    }

    public void SimulatePlayingFrame(GameplaySessionState s, float dt, KeyboardState currentKeys,
        KeyboardState previousKeys, Matrix camera)
    {
        var p = s.Player;
        var hpBefore = p.CurrentHealth;

        p.TickCooldowns(dt);
        p.TickGreenAura(dt, s.Enemies);

        if (s.ArenaIntroGraceSeconds > 0f)
            s.ArenaIntroGraceSeconds = MathHelper.Max(0f, s.ArenaIntroGraceSeconds - dt);

        var frame = new GameplayFrameContext(
            dt,
            s.PlayArea.Width,
            s.PlayArea.Height,
            camera,
            s.Enemies,
            s.Npc,
            s.Walls,
            s.PlayerProjectiles);

        var skillCountBefore = p.SkillFireCount;
        PlayerInput.Update(frame);
        if (p.SkillFireCount > skillCountBefore)
            _audio?.PlayShoot();

        foreach (var w in s.Walls)
            PhysicsHelper.ResolveHexCollision(ref p.Position, PlayerModel.CollisionRadius, w);

        _npcCarry.TryToggle(previousKeys, currentKeys, p, s.Npc);
        if (s.Npc.IsPickedUp)
            s.Npc.Position = p.Position + new Vector2(35f, -20f);

        var wallColliders =
            (from w in s.Walls select new Vector3(w.Center.X, w.Center.Y, w.Radius)).ToList();

        foreach (var e in s.Enemies)
            e.Update(dt);

        BossController.BrokenWallsThisFrame.Clear();
        _enemyAi.Update(dt, s.Enemies, p, s.Npc, s.PlayArea, wallColliders, s.EnemyProjectiles, s.Walls,
            arenaIntroGraceSeconds: s.ArenaIntroGraceSeconds);

        foreach (var e in s.Enemies.Where(x => !x.IsDead))
        {
            // Радиус коллизии = body radius архетипа. Это важно для босса (48f),
            // иначе он проходит сквозь укрытия и просто кладёт игрока в стенку.
            var collR = MathHelper.Max(e.Archetype.BodyRadius, 16f);
            foreach (var w in s.Walls)
                PhysicsHelper.ResolveHexCollision(ref e.Position, collR, w);
        }

        var projGrace = s.ArenaIntroGraceSeconds > 0f;
        _enemyProjSim.Update(dt, s.EnemyProjectiles, p, s.Npc, _floorFx, s.FloorSplatters,
            skipDamageToPlayerAndNpc: projGrace);
        _playerProjSim.Update(dt, s.PlayerProjectiles, s.Enemies, s.Walls, p, s.Npc, _floorFx, s.FloorSplatters);

        // Удаляем разрушенные укрытия (после слэма босса И после AoE-взрывов игрока).
        RemoveBrokenWalls(s);

        _deathLoot.Process(s.Enemies, s.Drops);
        foreach (var e in s.Enemies)
        {
            if (!e.IsDead || e.DeathBloodSfxPlayed) continue;
            e.DeathBloodSfxPlayed = true;
            _audio?.PlayBloodBoom();
        }

        _dropOrchestrator.Update(dt, p, s.Drops, s.FloatingTexts);

        _floorFx.Tick(s.FloorSplatters, dt);
        _wallSparkler.Tick(s.WallSparkles, s.Walls, dt);

        TickArenaTimer(s, dt);
        TickBossReinforcements(s, dt);

        var alive = 0;
        foreach (var e in s.Enemies)
            if (!e.IsDead) alive++;

        foreach (var g in s.Gates)
        {
            g.TryOpen(p, s.Npc, alive, s.TotalEnemiesInArena);
            if (!g.IsOpen) continue;
            TryAdvanceArena(s);
            return;
        }

        TickBossArenaShiftingZones(s, dt);

        ZoneResolver.RefreshPlayerZone(p, s.Zones);
        p.DamageMultiplier = ResolveDamageMultiplier(s.Npc);

        AccumulateShakeTrauma(s, hpBefore);
        _cameraShake.Tick(s.Camera, dt);

        TickWipeoutAdvance(s, dt);
    }

    private void TickArenaTimer(GameplaySessionState s, float dt)
    {
        if (s.ArenaTimer > 0f)
        {
            s.ArenaTimer = MathHelper.Max(0f, s.ArenaTimer - dt);
            return;
        }

        // Овертайм: лёгкий тик урона + периодические подкрепления-«Charger».
        s.OvertimeElapsed += dt;
        s.Player.TakeDamage(OvertimeDamagePerSecond * dt);

        _overtimeReinforcementClock += dt;
        if (_overtimeReinforcementClock < OvertimeReinforcementInterval) return;
        _overtimeReinforcementClock = 0f;
        SpawnOvertimeReinforcement(s);
    }

    private void RemoveBrokenWalls(GameplaySessionState s)
    {
        // Помечаются и слэмом босса, и AoE-взрывами игрока. Чистим один раз за кадр.
        foreach (var w in BossController.BrokenWallsThisFrame)
            s.Walls.Remove(w);
        BossController.BrokenWallsThisFrame.Clear();

        foreach (var w in _playerProjSim.BrokenWallsThisFrame)
            s.Walls.Remove(w);
        _playerProjSim.BrokenWallsThisFrame.Clear();
    }

    private static void SpawnOvertimeReinforcement(GameplaySessionState s)
    {
        // Чарджеры заходят с края арены, по 2 штуки сверху и снизу.
        var area = s.PlayArea;
        var rnd = new Random(unchecked(s.ArenaLayoutSeed ^ (int)(s.OvertimeElapsed * 1000f)));
        for (var i = 0; i < 2; i++)
        {
            var fromTop = rnd.Next(2) == 0;
            var pos = new Vector2(
                area.Left + 40f + (float)rnd.NextDouble() * (area.Width - 80f),
                fromTop ? area.Top + 40f : area.Bottom - 40f);
            s.Enemies.Add(new EnemyModel(pos, EnemyType.Charger));
        }
    }

    private static bool AnyBossAlive(List<EnemyModel> enemies)
    {
        foreach (var e in enemies)
            if (e.Type == EnemyType.Boss && !e.IsDead) return true;
        return false;
    }

    private static void TickBossReinforcements(GameplaySessionState s, float dt)
    {
        if (!s.Waves.IsBossArena || s.Waves.IsSecretMeatGrinder) return;
        if (s.ArenaTimer <= 0f) return;
        if (!AnyBossAlive(s.Enemies)) return;
        if (s.BossReinforcementWavesDone >= MaxBossReinforcementWaves) return;

        s.BossReinforcementTimer += dt;
        if (s.BossReinforcementTimer < BossReinforcementIntervalSeconds) return;
        s.BossReinforcementTimer = 0f;
        s.BossReinforcementWavesDone++;
        SpawnBossReinforcementWave(s);
    }

    private static void SpawnBossReinforcementWave(GameplaySessionState s)
    {
        var area = s.PlayArea;
        var rnd = new Random(unchecked(s.ArenaLayoutSeed ^ s.BossReinforcementWavesDone * 0x51ed));

        EnemyType[][] comps =
        {
            new[] { EnemyType.Melee, EnemyType.Melee, EnemyType.Shooter },
            new[] { EnemyType.Charger, EnemyType.Melee, EnemyType.Shooter },
            new[] { EnemyType.Melee, EnemyType.Sniper },
            new[] { EnemyType.Melee, EnemyType.Melee, EnemyType.Charger },
            new[] { EnemyType.Shooter, EnemyType.Shooter, EnemyType.Melee }
        };

        var pick = comps[rnd.Next(comps.Length)];
        foreach (var t in pick)
        {
            var fromTop = rnd.Next(2) == 0;
            var pos = new Vector2(
                area.Left + 70f + (float)rnd.NextDouble() * (area.Width - 140f),
                fromTop ? area.Top + 50f : area.Bottom - 50f);
            var e = new EnemyModel(pos, t);
            e.AttackCooldown = 1.5f + (float)rnd.NextDouble() * 1.35f;
            s.Enemies.Add(e);
        }
    }

    /// <summary>Бонус за живого здорового NPC. Штраф когда ранен/мёртв.</summary>
    private static float ResolveDamageMultiplier(NpcModel npc)
    {
        if (npc.IsDead) return 0.6f;
        if (npc.IsInjured) return 0.9f;
        return 1.3f;
    }

    private void TickWipeoutAdvance(GameplaySessionState s, float dt)
    {
        if (s.Enemies.Count == 0 || s.Enemies.All(e => e.IsDead))
        {
            s.WaveClearTimer += dt;
            if (s.WaveClearTimer >= WipeoutAdvanceDelay)
            {
                s.WaveClearTimer = 0f;
                if (s.Waves.IsSecretMeatGrinder)
                {
                    s.Waves.ExitSecretMeatGrinderMode();
                    s.SecretMeatVictory = true;
                    s.UiState = GameplayPhase.GameOver;
                    return;
                }

                TryAdvanceArena(s);
            }
        }
        else
        {
            s.WaveClearTimer = 0f;
        }
    }

    private static void TickBossArenaShiftingZones(GameplaySessionState s, float dt)
    {
        if (!s.Waves.IsBossArena || s.Zones.Count == 0) return;

        s.BossZoneShiftClock += dt;
        if (s.BossZoneShiftClock < BossArenaLayout.AnomalyShiftIntervalSeconds) return;
        s.BossZoneShiftClock = 0f;
        BossArenaLayout.CycleAnomalyZoneTypes(s.Zones);
    }

    /// <summary>Тряска по урону игрока, попаданиям, взрывам, AI-травме и отдаче; счётчики симуляторов сбрасываются.</summary>
    private void AccumulateShakeTrauma(GameplaySessionState s, float hpBefore)
    {
        var hpLost = hpBefore - s.Player.CurrentHealth;
        if (hpLost > 0)
            _cameraShake.Trigger(s.Camera, MathHelper.Clamp(hpLost / 22f, 0.12f, 0.52f));

        var enemyHits = _enemyProjSim.ConsumeHitsOnPlayer();
        if (enemyHits > 0)
            _cameraShake.Trigger(s.Camera, MathHelper.Clamp(enemyHits * 0.2f, 0.14f, 0.55f));

        var explosions = _playerProjSim.ConsumeExplosionEvents();
        if (explosions > 0)
            _cameraShake.Trigger(s.Camera, MathHelper.Clamp(explosions * 0.45f, 0.25f, 0.72f));

        var aiTrauma = _enemyAi.ConsumeTrauma();
        if (aiTrauma > 0)
            _cameraShake.Trigger(s.Camera, MathHelper.Clamp(aiTrauma * 0.65f, 0.08f, 0.55f));

        var recoil = _combat.ConsumeRecoilShake();
        if (recoil > 0f)
            _cameraShake.Trigger(s.Camera, MathHelper.Clamp(recoil * 0.38f, 0.04f, 0.22f));

        // Счётчики симуляторов сбрасываем, чтобы не копились между кадрами.
        _playerProjSim.ConsumeDirectHits();
        _playerProjSim.ConsumeAoeHits();
        _enemyProjSim.ConsumeHitsOnNpc();
    }
}

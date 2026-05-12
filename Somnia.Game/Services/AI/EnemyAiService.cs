using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.AI;

/// <summary>
/// AI врагов с диспатчем по архетипу.
/// Каждый <see cref="EnemyType"/> уходит в свой обработчик; общая логика (инфекция, выбор цели,
/// LOS, steering) выделена в private-методы.
/// </summary>
public sealed class EnemyAiService
{
    /// <summary>Сколько травмы добавлять камере, когда чарджер взрывается.</summary>
    public const float ChargerExplosionTrauma = 0.5f;

    private float _accumulatedTrauma;

    /// <summary>Накопленный за кадр запрос на тряску — оркестратор зачитывает и сбрасывает.</summary>
    public float ConsumeTrauma()
    {
        var t = _accumulatedTrauma;
        _accumulatedTrauma = 0f;
        return t;
    }

    public void Update(
        float dt,
        List<EnemyModel> enemies,
        PlayerModel player,
        NpcModel npc,
        Rectangle playArea,
        List<Vector3> walls,
        List<ProjectileModel> projectiles,
        List<HexagonModel>? destructibleWalls = null,
        float arenaIntroGraceSeconds = 0f)
    {
        foreach (var enemy in enemies.Where(x => !x.IsDead))
        {
            HandleInfection(enemy, dt, enemies);
            if (enemy.IsDummy || enemy.StunTimer > 0)
            {
                ClampToArena(enemy, playArea);
                continue;
            }

            var target = SelectTarget(enemy, player, npc);
            var dist = Vector2.Distance(enemy.Position, target.Position);

            switch (enemy.Type)
            {
                case EnemyType.Melee:
                    HandleMelee(enemy, target, dist, dt, walls, enemies, arenaIntroGraceSeconds);
                    break;
                case EnemyType.Shooter:
                    HandleShooter(enemy, player, dist, dt, walls, projectiles, enemies, arenaIntroGraceSeconds);
                    break;
                case EnemyType.Charger:
                    HandleCharger(enemy, target, dist, dt, enemies, arenaIntroGraceSeconds);
                    break;
                case EnemyType.Sniper:
                    HandleSniper(enemy, player, dist, dt, walls, projectiles, enemies, arenaIntroGraceSeconds);
                    break;
                case EnemyType.Boss:
                {
                    var strike = BossController.GetStrikeFocusWorld(enemy, player, npc);
                    var distBoss = Vector2.Distance(enemy.Position, strike);
                    BossController.Update(enemy, player, npc, distBoss, dt, projectiles, destructibleWalls);
                    break;
                }
            }

            // После любого мува врага — не даём ему уйти за пределы карты.
            // Граничные «стены» из BoundaryWalls стоят с зазором ~110px, но steering-сила
            // отбрасывания иногда выпихивает врагов в этот зазор — отсюда «за край».
            ClampToArena(enemy, playArea);
        }
    }

    /// <summary>Жёстко удерживает врага внутри игровой области с зазором на радиус тела.</summary>
    private static void ClampToArena(EnemyModel enemy, Rectangle playArea)
    {
        var margin = MathHelper.Max(enemy.Archetype.BodyRadius, 12f);
        enemy.Position = new Vector2(
            MathHelper.Clamp(enemy.Position.X, playArea.Left + margin, playArea.Right - margin),
            MathHelper.Clamp(enemy.Position.Y, playArea.Top + margin, playArea.Bottom - margin));
    }

    private static void HandleMelee(EnemyModel enemy, AiTarget target, float dist, float dt,
        List<Vector3> walls, List<EnemyModel> all, float arenaIntroGraceSeconds)
    {
        var a = enemy.Archetype;
        if (arenaIntroGraceSeconds <= 0f && dist <= a.MeleeReach && enemy.AttackCooldown <= 0)
        {
            target.ApplyDamage(a.MeleeDamage);
            enemy.AttackCooldown = a.AttackCooldown;
            return;
        }

        if (dist > a.MeleeReach * 0.8f)
            MoveSmart(enemy, target.Position, dt, a.MoveSpeed, walls, all);
    }

    private static void HandleShooter(EnemyModel enemy, PlayerModel player, float dist, float dt,
        List<Vector3> walls, List<ProjectileModel> projectiles, List<EnemyModel> all,
        float arenaIntroGraceSeconds)
    {
        var a = enemy.Archetype;

        if (dist > a.PreferredRange + 60f)
            MoveSmart(enemy, player.Position, dt, a.MoveSpeed, walls, all);
        else if (dist < a.PreferredRange - 80f)
            MoveSmart(enemy, enemy.Position + (enemy.Position - player.Position), dt, a.MoveSpeed * 0.6f, walls, all);

        if (arenaIntroGraceSeconds > 0f) return;
        if (enemy.AttackCooldown > 0 || dist >= a.EngageRange) return;
        if (!HasLineOfSight(enemy.Position, player.Position, walls)) return;

        var dir = Vector2.Normalize(player.Position - enemy.Position);
        projectiles.Add(new ProjectileModel(enemy.Position, dir * a.ProjectileSpeed, a.ProjectileRadius));
        enemy.AttackCooldown = a.AttackCooldown;
        enemy.MuzzleFlashTimer = 0.10f;
        enemy.MuzzleFlashDir = dir;
    }

    private void HandleCharger(EnemyModel enemy, AiTarget target, float dist, float dt,
        List<EnemyModel> all, float arenaIntroGraceSeconds)
    {
        var a = enemy.Archetype;
        var dir = target.Position - enemy.Position;
        if (dir != Vector2.Zero)
        {
            dir.Normalize();
            enemy.Position += dir * a.MoveSpeed * dt;
        }

        if (arenaIntroGraceSeconds <= 0f && dist <= a.MeleeReach && enemy.AttackCooldown <= 0)
        {
            target.ApplyDamage(a.MeleeDamage);
            if (a.ExplodesOnContact)
            {
                enemy.Health = 0;
                _accumulatedTrauma += ChargerExplosionTrauma;
            }
            enemy.AttackCooldown = a.AttackCooldown;
        }
    }

    private static void HandleSniper(EnemyModel enemy, PlayerModel player, float dist, float dt,
        List<Vector3> walls, List<ProjectileModel> projectiles, List<EnemyModel> all,
        float arenaIntroGraceSeconds)
    {
        var a = enemy.Archetype;

        if (dist < a.PreferredRange - 60f)
            MoveSmart(enemy, enemy.Position + (enemy.Position - player.Position), dt, a.MoveSpeed, walls, all);
        else if (dist > a.PreferredRange + 120f)
            MoveSmart(enemy, player.Position, dt, a.MoveSpeed * 0.6f, walls, all);

        if (arenaIntroGraceSeconds > 0f) return;

        // Шаг 1: армируем телеграф
        if (!enemy.TelegraphArmed
            && enemy.AttackCooldown <= 0
            && dist < a.EngageRange
            && HasLineOfSight(enemy.Position, player.Position, walls))
        {
            enemy.TelegraphArmed = true;
            enemy.TelegraphTimer = a.TelegraphTime;
            return;
        }

        // Шаг 2: телеграф идёт — ничего не делаем, View рисует линию
        if (enemy.IsTelegraphing) return;

        // Шаг 3: телеграф истёк — стреляем
        if (!enemy.TelegraphArmed) return;
        enemy.TelegraphArmed = false;

        if (!HasLineOfSight(enemy.Position, player.Position, walls))
        {
            enemy.AttackCooldown = 0.5f;
            return;
        }

        var dir = Vector2.Normalize(player.Position - enemy.Position);
        projectiles.Add(new ProjectileModel(enemy.Position, dir * a.ProjectileSpeed, a.ProjectileRadius)
        {
            LifeTime = 1.6f
        });
        enemy.AttackCooldown = a.AttackCooldown;
        enemy.MuzzleFlashTimer = 0.14f;
        enemy.MuzzleFlashDir = dir;
    }

    private static void MoveSmart(EnemyModel enemy, Vector2 target, float dt, float baseSpeed,
        List<Vector3> walls, List<EnemyModel> all)
    {
        var speed = enemy.SlowTimer > 0 ? baseSpeed * 0.3f : baseSpeed;
        var desired = target - enemy.Position;
        if (desired == Vector2.Zero) return;
        desired.Normalize();

        var avoidance = GetAvoidance(enemy.Position, desired, walls);
        var separation = GetSeparation(enemy, all);

        var combined = desired + avoidance * 4f + separation * 1.5f;
        if (combined == Vector2.Zero) return;

        combined.Normalize();
        enemy.Position += combined * speed * dt;
    }

    private static Vector2 GetAvoidance(Vector2 pos, Vector2 lookDir, List<Vector3> walls)
    {
        var force = Vector2.Zero;
        foreach (var w in walls)
        {
            var toWall = pos - new Vector2(w.X, w.Y);
            var dist = toWall.Length();
            var detectR = w.Z + 80f;
            if (!(dist < detectR) || !(dist > 0)) continue;

            var dot = Vector2.Dot(Vector2.Normalize(toWall), lookDir);
            if (dot >= 0) continue;

            var tangent = new Vector2(-toWall.Y, toWall.X);
            if (Vector2.Dot(tangent, lookDir) < 0) tangent = -tangent;
            force += Vector2.Normalize(tangent) * (1f - dist / detectR);
        }

        return force;
    }

    private static Vector2 GetSeparation(EnemyModel self, List<EnemyModel> others)
    {
        var force = Vector2.Zero;
        foreach (var other in others)
        {
            if (other == self || other.IsDead) continue;
            var diff = self.Position - other.Position;
            var len = diff.Length();
            if (len <= 0 || len >= 50f) continue;

            force += Vector2.Normalize(diff) * (1f - len / 50f);
        }

        return force;
    }

    private static bool HasLineOfSight(Vector2 a, Vector2 b, List<Vector3> walls)
    {
        foreach (var w in walls)
        {
            var c = new Vector2(w.X, w.Y);
            var ab = b - a;
            var len2 = ab.LengthSquared();
            if (len2 == 0) continue;

            var t = MathHelper.Clamp(Vector2.Dot(c - a, ab) / len2, 0f, 1f);
            var closest = a + ab * t;
            var diff = closest - c;
            diff.Y /= IsometricView.Squash;
            if (diff.Length() < w.Z - 5f) return false;
        }

        return true;
    }

    private static AiTarget SelectTarget(EnemyModel enemy, PlayerModel player, NpcModel npc)
    {
        if (enemy.Type == EnemyType.Shooter || enemy.Type == EnemyType.Sniper)
            return AiTarget.ForPlayer(player);

        var npcActive = !npc.IsPickedUp && !npc.IsDead;
        if (player.State == PlayerState.Carrying || !npcActive) return AiTarget.ForPlayer(player);

        return Vector2.Distance(enemy.Position, player.Position) <
               Vector2.Distance(enemy.Position, npc.Position)
            ? AiTarget.ForPlayer(player)
            : AiTarget.ForNpc(npc);
    }

    private static void HandleInfection(EnemyModel enemy, float dt, List<EnemyModel> enemies)
    {
        if (!enemy.IsInfected) return;

        enemy.InfectionTimer -= dt;
        if (enemy.InfectionTimer > 0) return;

        enemy.TakeDamage(25f, enemy.Position, 0f);
        enemy.IsInfected = false;

        foreach (var neighbor in enemies.Where(x => !x.IsDead && !x.IsInfected))
        {
            if (Vector2.Distance(enemy.Position, neighbor.Position) >= 150f) continue;
            neighbor.IsInfected = true;
            neighbor.InfectionTimer = 0.5f;
        }
    }

    /// <summary>Тонкая обёртка над двумя возможными целями, чтобы избежать null и боксинга.</summary>
    private readonly struct AiTarget
    {
        private readonly PlayerModel? _player;
        private readonly NpcModel? _npc;

        private AiTarget(PlayerModel? player, NpcModel? npc)
        {
            _player = player;
            _npc = npc;
        }

        public static AiTarget ForPlayer(PlayerModel player) => new(player, null);

        public static AiTarget ForNpc(NpcModel npc) => new(null, npc);

        public Vector2 Position => _player?.Position ?? _npc!.Position;

        public void ApplyDamage(float amount)
        {
            if (_player != null) _player.TakeDamage(amount);
            else _npc!.TakeDamage(amount);
        }
    }
}

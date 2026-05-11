using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.AI;
using Somnia.Game.Services.Combat;
using Somnia.Game.Services.Economy;
using Somnia.Game.Services.Npc;
using Somnia.Game.Services.Projectiles;
using Somnia.Game.Services.Waves;
using Somnia.Game.Services.World;
using Somnia.Game.Session;

namespace Somnia.Game.Controllers;

/// <summary>Оркестрация одного кадра игры: AI, снаряды, лут, гейты.</summary>
public sealed class GameplayOrchestrator
{
    private readonly ArenaLayoutGenerator _arenaGen = new();
    private readonly LineOfSightService _los = new();
    private readonly IPlayerCombatService _combat;
    private readonly PlayerProjectileSimulator _playerProjSim;
    private readonly EnemyProjectileSimulator _enemyProjSim = new();
    private readonly ResourceDropOrchestrator _dropOrchestrator = new();
    private readonly DeathLootSpawnService _deathLoot = new();
    private readonly NpcCarryInteractionService _npcCarry = new();
    private readonly EnemyAiService _enemyAi = new();

    public PlayerInputController PlayerInput { get; }

    public GameplayOrchestrator(PlayerModel player, IPlayerCombatService? combatOverride = null)
    {
        _combat = combatOverride ?? new PlayerCombatService(_los);
        _playerProjSim = new PlayerProjectileSimulator(_los);
        PlayerInput = new PlayerInputController(player, _combat);
    }

    public void RestartGame(GameplaySessionState s, int width, int height, Random? rng = null)
    {
        rng ??= new Random();

        s.UiState = 0;
        s.PlayArea = new Rectangle(0, 0, width, height);
        s.Waves = new WaveManager();
        s.ArenaLayoutSeed = rng.Next();

        ResetBodiesAndLists(s);
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
    }

    /// <summary>Переход на следующую арену через гейт. Возвращает false при завершении всех арен.</summary>
    public bool TryAdvanceArena(GameplaySessionState s, Random? rng = null)
    {
        rng ??= new Random();

        s.Waves.AdvanceArena();
        if (s.Waves.AllArenasCleared)
        {
            s.UiState = 2;
            return false;
        }

        ResetBodiesAndLists(s);
        s.ArenaLayoutSeed = rng.Next();
        RebuildArena(s);
        SpawnEnemiesForCurrentWave(s);
        return true;
    }

    private static void ResetBodiesAndLists(GameplaySessionState s)
    {
        s.Player.ResetForRun();
        s.Npc.ResetForRun();

        s.Player.Position = new Vector2(250f, s.PlayArea.Height / 2f);
        s.Player.SetState(PlayerState.Free);
        s.Npc.Position = new Vector2(250f, s.PlayArea.Height / 2f + 50f);

        s.PlayerProjectiles.Clear();
        s.EnemyProjectiles.Clear();
        s.Drops.Clear();
        s.FloatingTexts.Clear();

        s.Gates.Clear();
        s.Gates.Add(new GateModel(new Vector2(s.PlayArea.Width - 200f, s.PlayArea.Height / 2f)));
    }

    private void RebuildArena(GameplaySessionState s)
    {
        var layout = _arenaGen.Generate(s.PlayArea, s.ArenaLayoutSeed);
        s.Zones.Clear();
        s.Zones.AddRange(layout.Zones);
        s.Walls.Clear();
        s.Walls.AddRange(layout.Walls);
    }

    private static void SpawnEnemiesForCurrentWave(GameplaySessionState s)
    {
        s.Enemies.Clear();
        s.Enemies.AddRange(s.Waves.SpawnCurrentWave(s.PlayArea.Width, s.PlayArea.Height));
    }

    public void SimulatePlayingFrame(GameplaySessionState s, float dt, KeyboardState currentKeys,
        KeyboardState previousKeys, Matrix camera)
    {
        var p = s.Player;

        p.TickCooldowns(dt);
        p.TickGreenAura(dt, s.Enemies);

        var frame = new GameplayFrameContext(
            dt,
            s.PlayArea.Width,
            s.PlayArea.Height,
            camera,
            s.Enemies,
            s.Npc,
            s.Walls,
            s.PlayerProjectiles);

        PlayerInput.Update(frame);

        foreach (var w in s.Walls)
            PhysicsHelper.ResolveHexCollision(ref p.Position, 25f, w);

        _npcCarry.TryToggle(previousKeys, currentKeys, p, s.Npc);
        if (s.Npc.IsPickedUp)
            s.Npc.Position = p.Position + new Vector2(35f, -20f);

        var wallColliders =
            (from w in s.Walls select new Vector3(w.Center.X, w.Center.Y, w.Radius)).ToList();

        foreach (var e in s.Enemies)
            e.Update(dt);

        _enemyAi.Update(dt, s.Enemies, p, s.Npc, s.PlayArea, wallColliders, s.EnemyProjectiles);

        foreach (var e in s.Enemies.Where(x => !x.IsDead))
        {
            foreach (var w in s.Walls)
                PhysicsHelper.ResolveHexCollision(ref e.Position, 20f, w);
        }

        _enemyProjSim.Update(dt, s.EnemyProjectiles, p, s.Npc);
        _playerProjSim.Update(dt, s.PlayerProjectiles, s.Enemies, s.Walls);

        _deathLoot.Process(s.Enemies, s.Drops);
        _dropOrchestrator.Update(dt, p, s.Drops, s.FloatingTexts);

        foreach (var g in s.Gates)
        {
            g.TryOpen(p, s.Npc);
            if (!g.IsOpen) continue;
            TryAdvanceArena(s);
            return;
        }

        ZoneResolver.RefreshPlayerZone(p, s.Zones);
        p.DamageMultiplier = s.Npc.IsInjured ? 0.5f : 1f;
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.Waves;

namespace Somnia.Game.Session;

/// <summary>Mutable состояние одной загруженной сессии (арена).</summary>
public sealed class GameplaySessionState
{
    public Rectangle PlayArea { get; set; }

    public PlayerModel Player { get; set; } = null!;
    public NpcModel Npc { get; set; } = null!;
    public WaveManager Waves { get; set; } = null!;

    public List<EnemyModel> Enemies { get; } = new();
    public List<AnomalyZone> Zones { get; } = new();
    public List<HexagonModel> Walls { get; } = new();

    public List<ProjectileModel> EnemyProjectiles { get; } = new();
    public List<PlayerProjectileModel> PlayerProjectiles { get; } = new();

    public List<ResourceDropModel> Drops { get; } = new();
    public List<FloatingText> FloatingTexts { get; } = new();
    public List<GateModel> Gates { get; } = new();

    /// <summary>0 — игра, 1 — пауза, 2 — game over.</summary>
    public int UiState { get; set; }

    public KeyboardState PrevKeyboardState { get; set; }

    public int ArenaLayoutSeed { get; set; }
}

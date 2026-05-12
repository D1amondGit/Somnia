using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.Waves;

namespace Somnia.Game.Session;

/// <summary>Mutable состояние одной сессии (одного запуска игры).</summary>
public sealed class GameplaySessionState
{
    public Rectangle PlayArea { get; set; }

    public PlayerModel Player { get; set; } = null!;
    public NpcModel Npc { get; set; } = null!;
    public WaveManager Waves { get; set; } = null!;
    public CameraState Camera { get; } = new();

    public List<EnemyModel> Enemies { get; } = new();
    public List<AnomalyZone> Zones { get; } = new();
    public List<HexagonModel> Walls { get; } = new();

    public List<ProjectileModel> EnemyProjectiles { get; } = new();
    public List<PlayerProjectileModel> PlayerProjectiles { get; } = new();

    public List<ResourceDropModel> Drops { get; } = new();
    public List<FloatingText> FloatingTexts { get; } = new();
    public List<GateModel> Gates { get; } = new();

    /// <summary>См. <see cref="GameplayPhase"/>: 0 Playing, 1 Paused, 2 GameOver, 3 Title.</summary>
    public int UiState { get; set; } = GameplayPhase.Title;

    public KeyboardState PrevKeyboardState { get; set; }

    public int ArenaLayoutSeed { get; set; }

    /// <summary>Таймер задержки на автопереход арены после wipeout (живых врагов больше нет).</summary>
    public float WaveClearTimer { get; set; }

    /// <summary>Челлендж по времени: secs до овертайма. По истечении игрок начинает страдать.</summary>
    public float ArenaTimer { get; set; } = 90f;

    /// <summary>Сколько уже находимся в овертайме.</summary>
    public float OvertimeElapsed { get; set; }

    /// <summary>Брызги/следы на полу.</summary>
    public List<FloorSplatter> FloorSplatters { get; } = new();

    /// <summary>Искры на стенах для атмосферы.</summary>
    public List<WallSparkle> WallSparkles { get; } = new();

    /// <summary>Сколько врагов было заспавнено в текущей арене (для прогресса в HUD/гейте).</summary>
    public int TotalEnemiesInArena { get; set; }

    /// <summary>Секунды в начале арены: вражеские снаряды и дальние атаки не давят игрока/NPC.</summary>
    public float ArenaIntroGraceSeconds { get; set; }

    /// <summary>Накопление для смены типов цветных зон на босс-арене.</summary>
    public float BossZoneShiftClock { get; set; }

    /// <summary>Игрок выжил и зачистил секретную арену — показываем победный Game Over.</summary>
    public bool SecretMeatVictory { get; set; }

    /// <summary>Подкрепления на босс-арене: накопление до следующей волны.</summary>
    public float BossReinforcementTimer { get; set; }

    /// <summary>Сколько волн подкреплений уже выпущено (пока жив босс).</summary>
    public int BossReinforcementWavesDone { get; set; }

    /// <summary>Параметры процедурной текстуры пола (изолинии). Редактируй в отладчике для подбора вида.</summary>
    public FloorTextureSettings FloorTexture { get; set; } = new();
}

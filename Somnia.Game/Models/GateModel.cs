using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>
/// «Выход» с арены. Открывается, если игрок донёс NPC до точки И заодно
/// проредил противников до <see cref="MinKillFraction"/>. Это лечит баг-стратегию
/// «забрать NPC у спавна и пробежать мимо всех на +W».
/// </summary>
public class GateModel
{
    private const float TriggerRadius = 80f;

    public Vector2 Position { get; }
    public bool IsOpen { get; private set; }

    /// <summary>Сколько процентов исходных врагов должно быть убито, чтобы гейт открылся.</summary>
    public float MinKillFraction { get; set; } = 0.55f;

    /// <summary>Если true — гейт игнорирует <see cref="MinKillFraction"/> (например, в финале/боссе).</summary>
    public bool IgnoreKillRequirement { get; set; }

    public GateModel(Vector2 pos) => Position = pos;

    public void TryOpen(PlayerModel player, NpcModel npc, int aliveEnemies, int totalEnemies)
    {
        if (player.State != PlayerState.Carrying || npc.IsDead) return;
        if (Vector2.Distance(npc.Position, Position) >= TriggerRadius) return;

        if (!IgnoreKillRequirement && totalEnemies > 0)
        {
            var killed = totalEnemies - aliveEnemies;
            var killFrac = killed / (float)totalEnemies;
            if (killFrac < MinKillFraction) return;
        }

        IsOpen = true;
    }
}

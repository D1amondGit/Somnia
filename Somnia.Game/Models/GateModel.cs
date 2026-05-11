using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class GateModel
{
    private const float TriggerRadius = 80f;

    public Vector2 Position { get; }
    public bool IsOpen { get; private set; }

    public GateModel(Vector2 pos) => Position = pos;

    public void TryOpen(PlayerModel player, NpcModel npc)
    {
        if (player.State != PlayerState.Carrying || npc.IsDead) return;
        if (Vector2.Distance(npc.Position, Position) < TriggerRadius)
            IsOpen = true;
    }
}

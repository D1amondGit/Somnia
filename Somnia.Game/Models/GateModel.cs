using System.Numerics;

namespace Somnia.Game.Models
{
    public class GateModel
    {
        public Vector2 Position { get; }
        public bool IsOpen { get; private set; }
        private const float TriggerRadius = 80f;

        public GateModel(Vector2 pos) => Position = pos;

        public void TryOpen(PlayerModel player, NpcModel npc)
        {
            if (player.State != PlayerState.Carrying || npc.IsDead) return;
            if (Vector2.Distance(npc.Position, Position) < TriggerRadius)
                IsOpen = true;
        }
    }
}

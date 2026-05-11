using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Npc;

public sealed class NpcCarryInteractionService
{
    /// <summary>Снимает NPC с игрока (то же состояние, что и при отпускании по E).</summary>
    public static void DropCarriedNpc(PlayerModel player, NpcModel npc)
    {
        if (player.State != PlayerState.Carrying) return;
        player.SetState(PlayerState.Free);
        npc.IsPickedUp = false;
    }

    public void TryToggle(KeyboardState previous, KeyboardState current, PlayerModel player, NpcModel npc)
    {
        if (previous.IsKeyDown(Keys.E) || !current.IsKeyDown(Keys.E)) return;

        if (player.State == PlayerState.Free && Vector2.Distance(player.Position, npc.Position) < 80f)
        {
            player.SetState(PlayerState.Carrying);
            npc.IsPickedUp = true;
        }
        else if (player.State == PlayerState.Carrying)
            DropCarriedNpc(player, npc);
    }
}

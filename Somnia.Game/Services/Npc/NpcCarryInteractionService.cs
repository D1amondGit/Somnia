using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Npc;

public sealed class NpcCarryInteractionService
{
    public void TryToggle(KeyboardState previous, KeyboardState current, PlayerModel player, NpcModel npc)
    {
        if (previous.IsKeyDown(Keys.E) || !current.IsKeyDown(Keys.E)) return;

        if (player.State == PlayerState.Free && Vector2.Distance(player.Position, npc.Position) < 80f)
        {
            player.SetState(PlayerState.Carrying);
            npc.IsPickedUp = true;
        }
        else if (player.State == PlayerState.Carrying)
        {
            player.SetState(PlayerState.Free);
            npc.IsPickedUp = false;
        }
    }
}

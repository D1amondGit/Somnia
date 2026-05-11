using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

public static class ZoneResolver
{
    public static void RefreshPlayerZone(PlayerModel player, IReadOnlyList<AnomalyZone> zones)
    {
        player.CurrentZone = AnomalyType.Neutral;
        foreach (var z in zones)
        {
            if (z.ContainsPoint(player.Position))
                player.CurrentZone = z.Type;
        }
    }
}

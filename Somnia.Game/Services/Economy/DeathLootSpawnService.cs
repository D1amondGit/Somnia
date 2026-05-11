using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Economy;

public sealed class DeathLootSpawnService
{
    public void Process(IReadOnlyList<EnemyModel> enemies, List<ResourceDropModel> drops)
    {
        foreach (var e in enemies)
        {
            if (!e.IsDead || e.HasDropped) continue;
            e.HasDropped = true;
            drops.Add(new ResourceDropModel(e.Position + new Vector2(-15, 15), DropType.Health, 15f));
            drops.Add(new ResourceDropModel(e.Position + new Vector2(15, -15), DropType.Mana, 10f));
        }
    }
}

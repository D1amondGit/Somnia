using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Economy;

public sealed class ResourceDropOrchestrator
{
    public void Update(float dt, PlayerModel player, List<ResourceDropModel> drops, List<FloatingText> texts)
    {
        foreach (var d in drops)
            d.Update(player.Position, dt);

        foreach (var d in drops)
        {
            if (!d.Collected) continue;
            if (d.Type == DropType.Health)
            {
                player.CurrentHealth = MathHelper.Min(player.MaxHealth, player.CurrentHealth + d.Value);
                texts.Add(new FloatingText { Position = player.Position, Text = "+HP", Color = Color.Lime });
            }
            else if (d.Type == DropType.Mana)
            {
                player.CurrentMana = MathHelper.Min(player.MaxMana, player.CurrentMana + d.Value);
                texts.Add(new FloatingText { Position = player.Position, Text = "+MP", Color = Color.Cyan });
            }
        }

        drops.RemoveAll(d => d.Collected);

        foreach (var t in texts)
        {
            t.Position.Y -= 60f * dt;
            t.Timer -= dt;
        }

        texts.RemoveAll(t => t.Timer <= 0f);
    }
}

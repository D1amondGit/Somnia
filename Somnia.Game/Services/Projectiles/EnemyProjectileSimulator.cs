using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Projectiles;

/// <summary>Обновление снарядов врагов и столкновения с игроком/NPC.</summary>
public sealed class EnemyProjectileSimulator
{
    public void Update(float dt, List<ProjectileModel> projectiles, PlayerModel player, NpcModel npc)
    {
        for (var i = projectiles.Count - 1; i >= 0; i--)
        {
            var pr = projectiles[i];
            pr.Position += pr.Velocity * dt;
            pr.LifeTime -= dt;
            if (pr.LifeTime <= 0f)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            if (Vector2.Distance(pr.Position, player.Position) < 30f)
            {
                player.TakeDamage(10f);
                projectiles.RemoveAt(i);
            }
            else if (!npc.IsPickedUp && Vector2.Distance(pr.Position, npc.Position) < 30f)
            {
                npc.TakeDamage(10f);
                projectiles.RemoveAt(i);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Controllers
{
    public class EnemyController
    {
        public void Update(float dt, List<EnemyModel> enemies, PlayerModel player, NpcModel npc)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;

                Vector2 target = SelectTarget(enemy, player, npc);
                
                if (target != Vector2.Zero)
                {
                    Vector2 dir = target - enemy.Position;
                    if (dir.Length() > 2f)
                    {
                        dir.Normalize();
                        enemy.Position += dir * 150f * dt; // 150 - скорость врага
                    }
                }
            }
        }

        private Vector2 SelectTarget(EnemyModel e, PlayerModel p, NpcModel npc)
        {
            // Приоритет 1: Игрок, если он близко (< 200px)
            if (Vector2.Distance(e.Position, p.Position) < 200f) 
                return p.Position;
            
            // Приоритет 2: NPC, если он валяется на земле
            if (npc != null && !npc.IsPickedUp && !npc.IsDead && Vector2.Distance(e.Position, npc.Position) < 400f) 
                return npc.Position;
            
            return Vector2.Zero; // Иначе стоим
        }
    }
}
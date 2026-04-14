using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Controllers
{
    public class EnemyController
    {
        public void Update(float dt, List<EnemyModel> enemies, PlayerModel p, NpcModel npc)
        {
            foreach (var e in enemies)
            {
                if (e.IsDead) continue;

                // ЦЕПНОЕ ЗАРАЖЕНИЕ (Зеленая 3)
                if (e.IsInfected)
                {
                    e.InfectionTimer -= dt;
                    if (e.InfectionTimer <= 0)
                    {
                        e.TakeDamage(25f, e.Position, 0f); // Урон от взрыва вируса
                        e.IsInfected = false; // Вирус отработал
                        
                        // Заражаем соседей
                        foreach(var neighbor in enemies)
                        {
                            if (neighbor != e && !neighbor.IsDead && !neighbor.IsInfected && Vector2.Distance(e.Position, neighbor.Position) < 150f)
                            {
                                neighbor.IsInfected = true;
                                neighbor.InfectionTimer = 0.5f; // Задержка перед взрывом соседа
                            }
                        }
                    }
                }

                if (e.IsDummy) continue; 
                if (e.AttackCooldown > 0) e.AttackCooldown -= dt;
                if (e.StunTimer > 0) continue; 

                object targetEntity = GetClosestTarget(e, p, npc);
                if (targetEntity == null) continue;

                Vector2 targetPos = targetEntity == p ? p.Position : npc.Position;
                float dist = Vector2.Distance(e.Position, targetPos);

                if (dist <= 60f && e.AttackCooldown <= 0)
                {
                    if (targetEntity == p) p.TakeDamage(10f);
                    else npc.TakeDamage(10f);
                    e.AttackCooldown = 1.0f; 
                }
                else if (dist > 50f)
                {
                    float speed = e.SlowTimer > 0 ? 50f : 150f;
                    Vector2 dir = targetPos - e.Position;
                    if (dir != Vector2.Zero) e.Position += Vector2.Normalize(dir) * speed * dt;
                }
            }
        }

        private object GetClosestTarget(EnemyModel e, PlayerModel p, NpcModel npc)
        {
            if (p.State == PlayerState.Carrying || npc == null || npc.IsPickedUp || npc.IsDead) return p;
            float distToPlayer = Vector2.Distance(e.Position, p.Position);
            float distToNpc = Vector2.Distance(e.Position, npc.Position);
            return distToPlayer < distToNpc ? p : npc;
        }
    }
}
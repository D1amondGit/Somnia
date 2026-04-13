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
                if (e.AttackCooldown > 0) e.AttackCooldown -= dt;
                
                if (e.StunTimer > 0) continue; 

                Vector2 t = SelectTarget(e, p, npc);
                if (t == Vector2.Zero) continue;

                float dist = Vector2.Distance(e.Position, t);
                if (dist <= 60f && e.AttackCooldown <= 0) AttackTarget(e, t, p, npc);
                else if (dist > 50f) MoveTowards(e, t, dt);
            }
        }

        private void AttackTarget(EnemyModel e, Vector2 t, PlayerModel p, NpcModel npc)
        {
            if (t == p.Position) p.TakeDamage(10f);
            else if (npc != null && t == npc.Position) npc.TakeDamage(10f);
            e.AttackCooldown = 1.0f;
        }

        private void MoveTowards(EnemyModel e, Vector2 t, float dt)
        {
            float speed = e.SlowTimer > 0 ? 50f : 150f;
            e.Position += Vector2.Normalize(t - e.Position) * speed * dt;
        }

        private Vector2 SelectTarget(EnemyModel e, PlayerModel p, NpcModel npc)
        {
            // Если игрок несет НПС - все монстры агрятся на игрока с любого расстояния
            if (p.State == PlayerState.Carrying) return p.Position;
            
            // Если НПС валяется на земле и жив - идем его грызть
            if (npc != null && !npc.IsPickedUp && !npc.IsDead) return npc.Position;
            
            // Иначе (НПС мертв или его нет) - всегда преследуем игрока
            return p.Position;
        }
    }
}
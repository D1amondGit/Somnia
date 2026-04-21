using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Controllers
{
    public class EnemyController
    {
        public void Update(float dt, List<EnemyModel> enemies, PlayerModel p, NpcModel npc, Rectangle playArea, List<Vector3> walls, List<ProjectileModel> projectiles)
        {
            foreach (var e in enemies)
            {
                if (e.IsDead) continue;
                
                HandleInfection(e, dt, enemies); // Вирус (Зеленая зона 3)

                if (e.IsDummy || e.StunTimer > 0) continue; 
                if (e.AttackCooldown > 0) e.AttackCooldown -= dt;

                object target = GetTarget(e, p, npc);
                if (target == null) continue;

                Vector2 tPos = target == p ? p.Position : npc.Position;
                float dist = Vector2.Distance(e.Position, tPos);

                if (e.Type == EnemyType.Shooter) HandleShooter(e, p, dist, dt, walls, projectiles, enemies);
                else HandleMelee(e, target, p, npc, tPos, dist, dt, walls, enemies);
            }
        }

        private void HandleMelee(EnemyModel e, object t, PlayerModel p, NpcModel npc, Vector2 tPos, float dist, float dt, List<Vector3> walls, List<EnemyModel> allEnemies)
        {
            if (dist <= 60f && e.AttackCooldown <= 0) {
                if (t == p) p.TakeDamage(10f); else npc.TakeDamage(10f);
                e.AttackCooldown = 1.0f; 
            } else if (dist > 50f) {
                MoveSmart(e, tPos, dt, 120f, walls, allEnemies); // Вызов умного ИИ
            }
        }

        private void HandleShooter(EnemyModel e, PlayerModel p, float dist, float dt, List<Vector3> walls, List<ProjectileModel> projectiles, List<EnemyModel> allEnemies)
        {
            if (dist > 400f) MoveSmart(e, p.Position, dt, 100f, walls, allEnemies); 
            else if (dist < 250f) MoveSmart(e, e.Position + (e.Position - p.Position), dt, 100f, walls, allEnemies); 
            else if (e.AttackCooldown <= 0) { 
                Vector2 dir = p.Position - e.Position; 
                if (dir != Vector2.Zero) dir.Normalize();
                projectiles.Add(new ProjectileModel(e.Position, dir * 350f, 15f)); 
                e.AttackCooldown = 2.0f; 
            }
        }

        // НОВЫЙ УМНЫЙ ИИ (Steering Behaviors)
        private void MoveSmart(EnemyModel e, Vector2 target, float dt, float baseSpeed, List<Vector3> walls, List<EnemyModel> allEnemies)
        {
            float speed = e.SlowTimer > 0 ? baseSpeed * 0.3f : baseSpeed;
            Vector2 toTarget = target - e.Position; 
            if (toTarget == Vector2.Zero) return;

            Vector2 desiredDir = Vector2.Normalize(toTarget);
            Vector2 avoidance = Vector2.Zero;
            
            // 1. Огибание круглых стен (Obstacle Avoidance)
            float enemyRadius = 20f;
            foreach (var w in walls)
            {
                Vector2 wCenter = new Vector2(w.X, w.Y);
                Vector2 toWall = e.Position - wCenter; // Вектор ОТ стены к врагу
                float dist = toWall.Length();
                float detectionZone = w.Z + enemyRadius + 50f; // 50 пикселей - зона сканирования

                if (dist < detectionZone && dist > 0)
                {
                    // Чем ближе к стене, тем сильнее инстинкт самосохранения
                    float force = 1f - ((dist - w.Z - enemyRadius) / 50f);
                    
                    // Вычисляем обход по касательной (математика 9 класса!)
                    Vector2 tangent = new Vector2(-toWall.Y, toWall.X);
                    // Проверяем, с какой стороны стену обходить ближе
                    if (Vector2.Dot(tangent, desiredDir) < 0) tangent = -tangent; 
                    tangent.Normalize();

                    // Комбинируем выталкивание наружу и скольжение вдоль окружности
                    avoidance += (Vector2.Normalize(toWall) * force * 1.5f) + (tangent * force * 2.5f);
                }
            }

            // 2. Стайный интеллект (Separation - чтобы не слипались)
            Vector2 separation = Vector2.Zero;
            foreach (var other in allEnemies)
            {
                if (other == e || other.IsDead || other.IsDummy) continue;
                Vector2 toOther = e.Position - other.Position;
                float dist = toOther.Length();
                if (dist < 40f && dist > 0) // Держим дистанцию в 40 пикселей друг от друга
                {
                    separation += Vector2.Normalize(toOther) * (1f - dist / 40f) * 1.5f;
                }
            }

            // 3. Итоговый вектор движения
            Vector2 finalDir = desiredDir + avoidance + separation;
            if (finalDir != Vector2.Zero)
            {
                finalDir.Normalize();
                e.Position += finalDir * speed * dt;
            }
        }

        private object GetTarget(EnemyModel e, PlayerModel p, NpcModel npc)
        {
            if (e.Type == EnemyType.Shooter) return p; 
            bool npcUp = npc != null && !npc.IsPickedUp && !npc.IsDead;
            if (p.State == PlayerState.Carrying || !npcUp) return p;
            return Vector2.Distance(e.Position, p.Position) < Vector2.Distance(e.Position, npc.Position) ? p : npc;
        }

        private void HandleInfection(EnemyModel e, float dt, List<EnemyModel> enemies) {
            if (!e.IsInfected) return;
            e.InfectionTimer -= dt;
            if (e.InfectionTimer <= 0) {
                e.TakeDamage(25f, e.Position, 0f); e.IsInfected = false;
                foreach(var n in enemies) if (n != e && !n.IsDead && !n.IsInfected && Vector2.Distance(e.Position, n.Position) < 150f) 
                { n.IsInfected = true; n.InfectionTimer = 0.5f; }
            }
        }
    }
}
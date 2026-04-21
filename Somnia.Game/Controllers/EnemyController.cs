using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using System.Collections.Generic;
using System.Linq;

namespace Somnia.Game.Controllers
{
    public class EnemyController
    {
        public void Update(float dt, List<EnemyModel> enemies, PlayerModel p, NpcModel npc, Rectangle playArea, List<Vector3> walls, List<ProjectileModel> projs)
        {
            foreach (var e in enemies.Where(x => !x.IsDead)) {
                HandleInfection(e, dt, enemies);
                if (e.IsDummy || e.StunTimer > 0) continue; 
                
                object target = GetTarget(e, p, npc);
                if (target == null) continue;

                Vector2 tPos = target == p ? p.Position : npc.Position;
                float dist = Vector2.Distance(e.Position, tPos);

                if (e.Type == EnemyType.Shooter) HandleShooter(e, p, dist, dt, walls, projs, enemies);
                else HandleMelee(e, tPos, dist, dt, walls, enemies, target == p ? p : null, target == npc ? npc : null);
            }
        }

        private void HandleMelee(EnemyModel e, Vector2 tPos, float dist, float dt, List<Vector3> w, List<EnemyModel> a, PlayerModel p, NpcModel npc)
        {
            if (dist <= 65f && e.AttackCooldown <= 0) {
                if (p != null) p.TakeDamage(10f); else if (npc != null) npc.TakeDamage(10f);
                e.AttackCooldown = 1.0f; 
            } else if (dist > 50f) MoveSmart(e, tPos, dt, 150f, w, a); 
        }

        private void HandleShooter(EnemyModel e, PlayerModel p, float dist, float dt, List<Vector3> w, List<ProjectileModel> projs, List<EnemyModel> a)
        {
            if (dist > 400f) MoveSmart(e, p.Position, dt, 110f, w, a); 
            // ПРОВЕРКА ВИДИМОСТИ: Если стена мешает — выстрела не будет
            if (e.AttackCooldown <= 0 && dist < 800f && HasLineOfSight(e.Position, p.Position, w)) { 
                Vector2 dir = Vector2.Normalize(p.Position - e.Position);
                projs.Add(new ProjectileModel(e.Position, dir * 450f, 10f));
                e.AttackCooldown = 2.2f; 
            }
        }

        private void MoveSmart(EnemyModel e, Vector2 target, float dt, float baseSpeed, List<Vector3> walls, List<EnemyModel> allEnemies)
        {
            float speed = e.SlowTimer > 0 ? baseSpeed * 0.3f : baseSpeed;
            Vector2 desired = target - e.Position; 
            if (desired == Vector2.Zero) return;
            desired.Normalize();

            Vector2 avoidance = GetAvoidance(e.Position, desired, walls);
            Vector2 separation = GetSeparation(e, allEnemies);

            // Приоритет обхода стен (3.0) выше, чем тяга к цели (1.0)
            Vector2 final = desired + avoidance * 4.0f + separation * 1.5f;
            if (final != Vector2.Zero) { 
                final.Normalize(); 
                e.Position += final * speed * dt; 
            }
        }

        private Vector2 GetAvoidance(Vector2 pos, Vector2 lookDir, List<Vector3> walls)
        {
            Vector2 force = Vector2.Zero;
            foreach (var w in walls) {
                Vector2 toWall = pos - new Vector2(w.X, w.Y);
                float dist = toWall.Length();
                float detectR = w.Z + 80f; // Дистанция обнаружения стены
                if (dist < detectR && dist > 0) {
                    float dot = Vector2.Dot(Vector2.Normalize(toWall), lookDir);
                    if (dot < 0) { // Стена впереди по курсу
                        Vector2 tangent = new Vector2(-toWall.Y, toWall.X);
                        if (Vector2.Dot(tangent, lookDir) < 0) tangent = -tangent;
                        force += Vector2.Normalize(tangent) * (1f - dist / detectR);
                    }
                }
            }
            return force;
        }

        private Vector2 GetSeparation(EnemyModel e, List<EnemyModel> others)
        {
            Vector2 force = Vector2.Zero;
            foreach (var o in others) {
                if (o == e || o.IsDead) continue;
                Vector2 diff = e.Position - o.Position;
                if (diff.Length() < 50f && diff.Length() > 0) 
                    force += Vector2.Normalize(diff) * (1f - diff.Length() / 50f);
            }
            return force;
        }

        private bool HasLineOfSight(Vector2 a, Vector2 b, List<Vector3> walls) {
            foreach (var w in walls) {
                Vector2 c = new Vector2(w.X, w.Y);
                Vector2 ab = b - a; float len2 = ab.LengthSquared();
                if (len2 == 0) continue;
                float t = MathHelper.Clamp(Vector2.Dot(c - a, ab) / len2, 0f, 1f);
                Vector2 closest = a + ab * t;
                Vector2 diff = closest - c; diff.Y /= 0.7f;
                if (diff.Length() < w.Z - 5f) return false;
            } return true;
        }

        private object GetTarget(EnemyModel e, PlayerModel p, NpcModel npc)
        {
            if (e.Type == EnemyType.Shooter) return p; 
            bool npcActive = npc != null && !npc.IsPickedUp && !npc.IsDead;
            if (p.State == PlayerState.Carrying || !npcActive) return p;
            return Vector2.Distance(e.Position, p.Position) < Vector2.Distance(e.Position, npc.Position) ? p : npc;
        }

        private void HandleInfection(EnemyModel e, float dt, List<EnemyModel> enemies) {
            if (!e.IsInfected) return;
            e.InfectionTimer -= dt;
            if (e.InfectionTimer <= 0) {
                e.TakeDamage(25f, e.Position, 0f); e.IsInfected = false;
                foreach(var n in enemies.Where(x => !x.IsDead && !x.IsInfected)) {
                    if (Vector2.Distance(e.Position, n.Position) < 150f) { n.IsInfected = true; n.InfectionTimer = 0.5f; }
                }
            }
        }
    }
}
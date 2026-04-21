using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }
    public enum AnomalyType { Red, Blue, Green, Neutral }

    public class PlayerModel
    {
        public Vector2 Position, FacingDir = Vector2.UnitX;
        public PlayerState State { get; private set; } = PlayerState.Free;
        public AnomalyType CurrentZone = AnomalyType.Neutral;
        public float CurrentHealth = 100f, MaxHealth = 100f, CurrentMana = 100f, DamageMultiplier = 1.0f;
        public bool IsDead => CurrentHealth <= 0;
        public int ActiveSlot = 0; 
        public float Cd1, Cd2, Cd3, MaxCd1 = 0.5f, MaxCd2 = 2f, MaxCd3 = 5f;
        public bool IsDashing => _dashTimer > 0;
        public bool IsAttacking => _attackTimer > 0;
        public float GreenAuraTimer { get; private set; } 
        private float _dashTimer, _dashCd, _attackTimer;

        public PlayerModel(Vector2 start) { Position = start; }

        public void SetState(PlayerState s) => State = s;
        public void TakeDamage(float a) => CurrentHealth = Math.Max(0, CurrentHealth - a);
        public void UpdateFacing(Vector2 d) { if (d != Vector2.Zero) { d.Normalize(); FacingDir = d; } }
        public void StartDash() { if (_dashCd <= 0) { _dashTimer = 0.2f; _dashCd = 1f; } }
        private bool ConsumeMana(float a) { if (CurrentMana >= a) { CurrentMana -= a; return true; } return false; }

        public void Update(float dt)
        {
            CurrentMana = Math.Min(100f, CurrentMana + 10f * dt);
            if (Cd1 > 0) Cd1 -= dt; if (Cd2 > 0) Cd2 -= dt; if (Cd3 > 0) Cd3 -= dt;
            if (_dashCd > 0) _dashCd -= dt; if (_dashTimer > 0) _dashTimer -= dt;
            if (_attackTimer > 0) _attackTimer -= dt;
        }

        public void UpdateSkills(float dt, List<EnemyModel> enemies)
        {
            if (GreenAuraTimer <= 0) return;
            GreenAuraTimer -= dt;
            foreach (var e in enemies) {
                if (e.IsDead || Vector2.Distance(Position, e.Position) >= 200f) continue;
                Vector2 push = e.Position - Position;
                if (push != Vector2.Zero) { e.Position += Vector2.Normalize(push) * 400f * dt; e.TakeDamage(15f * dt, Position, 0f); }
            }
        }

        public void UseActiveSkill(Vector2 target, List<EnemyModel> enemies, NpcModel npc, List<HexagonModel> walls)
        {
            if (State == PlayerState.Carrying || (ActiveSlot == 0 && Cd1 > 0) || (ActiveSlot == 1 && Cd2 > 0) || (ActiveSlot == 2 && Cd3 > 0)) return;
            float m = DamageMultiplier;
            Vector2 dir = target != Vector2.Zero ? Vector2.Normalize(target) : Vector2.UnitX;

            bool s = CurrentZone switch {
                AnomalyType.Red => UseRed(dir, enemies, npc, m, walls),
                AnomalyType.Blue => UseBlue(dir, enemies, m, walls),
                AnomalyType.Green => UseGreen(dir, enemies, m, walls),
                AnomalyType.Neutral => UseNeutral(dir, enemies, m, walls),
                _ => false
            };
            if (s) { _attackTimer = 0.25f; if (ActiveSlot == 0) Cd1 = MaxCd1; else if (ActiveSlot == 1) Cd2 = MaxCd2; else Cd3 = MaxCd3; }
        }

        private bool HasLineOfSight(Vector2 a, Vector2 b, List<HexagonModel> walls) {
            foreach (var w in walls) {
                Vector2 c = new Vector2(w.Center.X, w.Center.Y);
                Vector2 ap = c - a; Vector2 ab = b - a; float ab2 = ab.LengthSquared();
                float t = ab2 == 0 ? 0 : MathHelper.Clamp(Vector2.Dot(ap, ab) / ab2, 0f, 1f);
                Vector2 diff = (a + ab * t) - c; diff.Y /= 0.7f;
                if (diff.Length() < w.Radius) return false;
            } return true;
        }

        private bool UseRed(Vector2 dir, List<EnemyModel> enemies, NpcModel npc, float m, List<HexagonModel> w) {
            if (ActiveSlot == 0 && ConsumeMana(10f)) { 
                var hits = new List<EnemyModel>();
                // Дробовик (увеличили угол до 0.4f, чтобы проще попадать)
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 250f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.4f && HasLineOfSight(Position, e.Position, w)) hits.Add(e);
                if (hits.Count > 0) { float dmg = (100f * m) / hits.Count; foreach (var e in hits) e.TakeDamage(dmg, Position, 900f); }
                MaxCd1 = 0.5f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(20f)) { 
                // ЛАССО: Тянет и врага и NPC
                object t = GetClosestEntity(dir, enemies, npc, 800f, 0.4f, w);
                if (t is EnemyModel em) { em.Velocity += Vector2.Normalize(Position - em.Position) * 1500f; em.TakeDamage(10f, Position, 0f); }
                if (t is NpcModel nm) nm.Position = Vector2.Lerp(nm.Position, Position, 0.85f); // ТЕПЕРЬ ТЯНЕТ NPC!
                MaxCd2 = 2f; return true; 
            }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { 
                object t = GetClosestEntity(dir, enemies, null, 2000f, 0.95f, w);
                if (t is EnemyModel em) em.TakeDamage(200f * m, Position, 200f);
                MaxCd3 = 5f; return true; 
            } return false;
        }

        private bool UseGreen(Vector2 dir, List<EnemyModel> enemies, float m, List<HexagonModel> w) {
            if (ActiveSlot == 0 && ConsumeMana(10f)) {
                object t = GetClosestEntity(dir, enemies, null, 1000f, 0.8f, w);
                if (t is EnemyModel em) em.TakeDamage(40f * m, Position, 200f);
                MaxCd1 = 0.8f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(30f)) { GreenAuraTimer = 4f; MaxCd2 = 5f; return true; }
            if (ActiveSlot == 2 && ConsumeMana(40f)) { 
                object t = GetClosestEntity(dir, enemies, null, 1000f, 0.8f, w);
                if (t is EnemyModel em) { em.IsInfected = true; em.InfectionTimer = 0.1f; }
                MaxCd3 = 4f; return true; 
            } return false;
        }

        private bool UseBlue(Vector2 dir, List<EnemyModel> enemies, float m, List<HexagonModel> w) {
            if (ActiveSlot == 0 && ConsumeMana(5f)) {
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 150f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.5f && HasLineOfSight(Position, e.Position, w)) e.TakeDamage(15f * m, Position, 100f);
                MaxCd1 = 0.2f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(25f)) { 
                StartDash(); foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 250f && HasLineOfSight(Position, e.Position, w)) e.StunTimer = 2.5f;
                MaxCd2 = 3f; return true; 
            }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { foreach(var e in enemies) e.SlowTimer=4f; MaxCd3 = 10f; return true; }
            return false;
        }

        private bool UseNeutral(Vector2 dir, List<EnemyModel> enemies, float m, List<HexagonModel> w) {
            if (ActiveSlot == 0 && ConsumeMana(5f)) { 
                object t = GetClosestEntity(dir, enemies, null, 600f, 0.90f, w);
                if (t is EnemyModel em) em.TakeDamage(25f * m, Position, 300f);
                MaxCd1 = 0.3f; return true; 
            } return false;
        }

        private object GetClosestEntity(Vector2 aimDir, List<EnemyModel> enemies, NpcModel npc, float maxRange, float minDot, List<HexagonModel> w) {
            object best = null; float bestDot = minDot;
            foreach (var e in enemies) {
                Vector2 toE = e.Position - Position;
                if (toE.Length() > 0 && toE.Length() < maxRange && HasLineOfSight(Position, e.Position, w)) {
                    float dot = Vector2.Dot(aimDir, Vector2.Normalize(toE));
                    if (dot > bestDot) { bestDot = dot; best = e; }
                }
            } 
            if (npc != null && !npc.IsPickedUp) {
                Vector2 toN = npc.Position - Position;
                if (toN.Length() > 0 && toN.Length() < maxRange && HasLineOfSight(Position, npc.Position, w)) {
                    float dot = Vector2.Dot(aimDir, Vector2.Normalize(toN));
                    if (dot > bestDot) { bestDot = dot; best = npc; }
                }
            } return best;
        }
    }
}
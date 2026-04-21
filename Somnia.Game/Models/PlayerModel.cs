using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }
    public enum AnomalyType { Red, Blue, Neutral }
    public enum GameState { Playing, Paused, GameOver }

    public class PlayerModel
    {
        public Vector2 Position { get; set; }
        public PlayerState State { get; private set; } = PlayerState.Free;
        public AnomalyType CurrentZone { get; set; } = AnomalyType.Neutral;
        public Vector2 FacingDir { get; private set; } = Vector2.UnitX;
        
        public float CurrentHealth { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public float CurrentMana { get; set; } = 100f;
        public bool IsDead => CurrentHealth <= 0;
        public int ActiveSlot { get; set; } = 0; 
        
        public float Cd1 { get; private set; }
        public float Cd2 { get; private set; }
        public float Cd3 { get; private set; }
        public float MaxCd1 { get; private set; }
        public float MaxCd2 { get; private set; }
        public float MaxCd3 { get; private set; }
        
        public bool IsDashing { get; private set; }
        public bool IsAttacking => _attackTimer > 0;
        public float GreenAuraTimer { get; private set; }

        private float _dashTimer, _dashCd, _attackTimer;
        private Vector2 _dashDir;

        public PlayerModel(Vector2 start) { Position = start; }

        public void SetState(PlayerState s) => State = s;
        public void TakeDamage(float a) => CurrentHealth = Math.Max(0, CurrentHealth - a);
        public void UpdateFacing(Vector2 d) { if (d != Vector2.Zero) { d.Normalize(); FacingDir = d; } }

        // ЗАМЕНИЛИ w, h НА playArea, А walls НА Vector3 (круги)
        public void Move(Vector2 dir, float dt, Rectangle playArea, List<HexagonModel> walls)
        {
            CurrentMana = Math.Min(100f, CurrentMana + 10f * dt);
            if (Cd1 > 0) Cd1 -= dt; if (Cd2 > 0) Cd2 -= dt; if (Cd3 > 0) Cd3 -= dt;
            if (_dashCd > 0) _dashCd -= dt;
            if (_dashTimer > 0) { _dashTimer -= dt; if (_dashTimer <= 0) IsDashing = false; }
            if (_attackTimer > 0) _attackTimer -= dt;

            Vector2 move = IsDashing ? _dashDir * 2000f : (dir != Vector2.Zero ? Vector2.Normalize(dir) * (State == PlayerState.Carrying ? 250f : 500f) : Vector2.Zero);
            Position += move * dt;

            Vector2 center = Position + new Vector2(25, 25);
            foreach (var w in walls) PhysicsHelper.ResolveHexCollision(ref center, 25f, w);
            Position = center - new Vector2(25, 25);

            Position = new Vector2(
                MathHelper.Clamp(Position.X, playArea.X, playArea.Right - 50),
                MathHelper.Clamp(Position.Y, playArea.Y, playArea.Bottom - 50)
            );
        }

        public void UpdateSkills(float dt, List<EnemyModel> enemies, float dmgMult) {
            if (GreenAuraTimer > 0) {
                GreenAuraTimer -= dt;
                foreach (var e in enemies) {
                    if (Vector2.Distance(Position, e.Position) < 200f) {
                        // ПОЧИНКА: Используем встроенную физику отдачи с силой 600
                        e.TakeDamage(15f * dmgMult * dt, Position, 600f);
                    }
                }
            }
        }

        public enum AnomalyType { Red, Blue, Green, Neutral } // Добавили Green отдельно

// ... внутри PlayerModel ...
        public void UseActiveSkill(Vector2 target, List<EnemyModel> enemies, NpcModel npc)
        {
            if (State == PlayerState.Carrying || (ActiveSlot == 0 && Cd1 > 0) || (ActiveSlot == 1 && Cd2 > 0) || (ActiveSlot == 2 && Cd3 > 0)) return;

            float dmgMult = (npc != null && !npc.IsDead && npc.Health < 50f) ? 0.5f : 1f;
            Vector2 normDir = target != Vector2.Zero ? Vector2.Normalize(target) : Vector2.UnitX;

            // Теперь каждая зона вызывает свои методы
            bool success = CurrentZone switch {
                AnomalyType.Red => UseRed(normDir, enemies, npc, dmgMult),
                AnomalyType.Blue => UseBlue(normDir, enemies, dmgMult),
                AnomalyType.Green => UseGreen(normDir, enemies, dmgMult),
                AnomalyType.Neutral => UseNeutral(normDir, enemies, dmgMult), // Базовые атаки
                _ => false
            };
    
            if (success) { 
                _attackTimer = 0.15f; 
                if (ActiveSlot == 0) Cd1 = MaxCd1; else if (ActiveSlot == 1) Cd2 = MaxCd2; else Cd3 = MaxCd3; 
            }
        }

// Новый метод для нейтральной зоны (обычные выстрелы/удары)
        private bool UseNeutral(Vector2 dir, List<EnemyModel> enemies, float mult) {
            if (ActiveSlot == 0 && ConsumeMana(5f)) {
                object t = GetClosestEntity(dir, enemies, null, 800f, 0.95f);
                if (t is EnemyModel em) em.TakeDamage(25f * mult, Position, 100f);
                MaxCd1 = 0.3f; return true;
            }
            return false;
        }

        private bool UseRed(Vector2 dir, List<EnemyModel> enemies, NpcModel npc, float mult) {
            if (ActiveSlot == 0 && ConsumeMana(10f)) { 
                var hits = new List<EnemyModel>();
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 200f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.5f) hits.Add(e);
                if (hits.Count > 0) { float dmg = (100f * mult) / hits.Count; foreach (var e in hits) e.TakeDamage(dmg, Position, 900f); }
                MaxCd1 = 0.5f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(20f)) { 
                object t = GetClosestEntity(dir, enemies, npc, 600f, 0.7f);
                if (t is EnemyModel em) em.Position = Vector2.Lerp(em.Position, Position, 0.85f);
                if (t is NpcModel nm) nm.Position = Vector2.Lerp(nm.Position, Position, 0.85f);
                MaxCd2 = 2f; return true; 
            }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { 
                object target = GetClosestEntity(dir, enemies, null, 2000f, 0.95f);
                if (target is EnemyModel em) em.TakeDamage(200f * mult, Position, 0f);
                MaxCd3 = 5f; return true; 
            } return false;
        }

        private bool UseGreen(Vector2 dir, List<EnemyModel> enemies, float mult) {
            if (ActiveSlot == 0 && ConsumeMana(10f)) { 
                object t = GetClosestEntity(dir, enemies, null, 1000f, 0.9f);
                if (t is EnemyModel em) em.TakeDamage(40f * mult, Position, 200f);
                MaxCd1 = 0.8f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(30f)) { GreenAuraTimer = 4f; MaxCd2 = 5f; return true; }
            if (ActiveSlot == 2 && ConsumeMana(40f)) { 
                object target = GetClosestEntity(dir, enemies, null, 1000f, 0.9f);
                if (target is EnemyModel em) { em.IsInfected = true; em.InfectionTimer = 0.1f; }
                MaxCd3 = 4f; return true; 
            } return false;
        }

        private bool UseBlue(Vector2 dir, List<EnemyModel> enemies, float mult) {
            if (ActiveSlot == 0 && ConsumeMana(5f)) {
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 100f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.5f) e.TakeDamage(15f * mult, Position, 100f);
                MaxCd1 = 0.2f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(25f)) { 
                if (!IsDashing && _dashCd <= 0) { IsDashing = true; _dashTimer = 0.2f; _dashCd = 1.5f; _dashDir = dir; }
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 250f) e.StunTimer = 2.5f;
                MaxCd2 = 3f; return true; 
            }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { foreach(var e in enemies) e.SlowTimer=4f; MaxCd3 = 10f; return true; }
            return false;
        }

        private bool ConsumeMana(float a) { if (CurrentMana >= a) { CurrentMana -= a; return true; } return false; }

        private EnemyModel GetClosestEnemy(Vector2 dir, List<EnemyModel> enemies, float range, float angle) {
            EnemyModel best = null; float bestDot = angle;
            foreach (var e in enemies) {
                Vector2 toE = e.Position - Position;
                if (toE.Length() > 0 && toE.Length() < range) {
                    float dot = Vector2.Dot(dir, Vector2.Normalize(toE));
                    if (dot > bestDot) { bestDot = dot; best = e; }
                }
            } return best;
        }

        private object GetClosestEntity(Vector2 dir, List<EnemyModel> enemies, NpcModel npc, float maxRange, float minDot) {
            object best = GetClosestEnemy(dir, enemies, maxRange, minDot);
            float bestD = best != null ? Vector2.Distance(Position, ((EnemyModel)best).Position) : maxRange;
            if (npc != null && !npc.IsPickedUp) {
                Vector2 toN = npc.Position - Position;
                if (toN.Length() > 0 && toN.Length() < bestD && Vector2.Dot(dir, Vector2.Normalize(toN)) > minDot) best = npc;
            } return best;
        }
    }
}
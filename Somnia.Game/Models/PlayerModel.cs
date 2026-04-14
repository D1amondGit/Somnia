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

        public void Move(Vector2 dir, float dt, int w, int h)
        {
            CurrentMana = Math.Min(100f, CurrentMana + 10f * dt);
            if (Cd1 > 0) Cd1 -= dt; if (Cd2 > 0) Cd2 -= dt; if (Cd3 > 0) Cd3 -= dt;
            if (_dashCd > 0) _dashCd -= dt;
            if (_dashTimer > 0) { _dashTimer -= dt; if (_dashTimer <= 0) IsDashing = false; }
            if (_attackTimer > 0) _attackTimer -= dt;

            Vector2 move = IsDashing ? _dashDir * 2000f : (dir != Vector2.Zero ? Vector2.Normalize(dir) * (State == PlayerState.Carrying ? 250f : 500f) : Vector2.Zero);
            Position = new Vector2(MathHelper.Clamp(Position.X + move.X * dt, 0, w - 50), MathHelper.Clamp(Position.Y + move.Y * dt, 0, h - 50));
        }

        // Обновление длительных навыков (Аура)
        public void UpdateSkills(float dt, List<EnemyModel> enemies)
        {
            if (GreenAuraTimer > 0)
            {
                GreenAuraTimer -= dt;
                foreach (var e in enemies)
                {
                    if (Vector2.Distance(Position, e.Position) < 200f)
                    {
                        Vector2 pushDir = e.Position - Position;
                        if (pushDir != Vector2.Zero)
                        {
                            e.Position += Vector2.Normalize(pushDir) * 400f * dt;
                            e.TakeDamage(15f * dt, Position, 0f);
                        }
                    }
                }
            }
        }

        public void UseActiveSkill(Vector2 target, List<EnemyModel> enemies, NpcModel npc)
        {
            if (State == PlayerState.Carrying) return;
            if (ActiveSlot == 0 && Cd1 > 0) return;
            if (ActiveSlot == 1 && Cd2 > 0) return;
            if (ActiveSlot == 2 && Cd3 > 0) return;

            Vector2 normDir = target != Vector2.Zero ? Vector2.Normalize(target) : Vector2.UnitX;
            bool success = CurrentZone == AnomalyType.Red ? UseRed(normDir, enemies, npc) : 
                           CurrentZone == AnomalyType.Blue ? UseBlue(normDir, enemies) : UseGreen(normDir, enemies);
            
            if (success) {
                _attackTimer = 0.15f; 
                if (ActiveSlot == 0) Cd1 = MaxCd1; else if (ActiveSlot == 1) Cd2 = MaxCd2; else Cd3 = MaxCd3;
            }
        }

        private bool UseRed(Vector2 dir, List<EnemyModel> enemies, NpcModel npc)
        {
            if (ActiveSlot == 0 && ConsumeMana(10f)) { 
                var hits = new List<EnemyModel>();
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 200f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.5f) hits.Add(e);
                if (hits.Count > 0) { float dmg = 100f / hits.Count; foreach (var e in hits) e.TakeDamage(dmg, Position, 900f); }
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
                if (target is EnemyModel em) em.TakeDamage(200f, Position, 0f);
                MaxCd3 = 5f; return true; 
            }
            return false;
        }

        private bool UseGreen(Vector2 dir, List<EnemyModel> enemies)
        {
            if (ActiveSlot == 0 && ConsumeMana(10f)) {
                object target = GetClosestEntity(dir, enemies, null, 1000f, 0.9f);
                if (target is EnemyModel em) em.TakeDamage(40f, Position, 200f);
                MaxCd1 = 0.8f; return true; 
            }
            if (ActiveSlot == 1 && ConsumeMana(30f)) { 
                GreenAuraTimer = 4f; 
                MaxCd2 = 5f; return true; 
            }
            if (ActiveSlot == 2 && ConsumeMana(40f)) { 
                object target = GetClosestEntity(dir, enemies, null, 1000f, 0.9f);
                if (target is EnemyModel em) {
                    em.IsInfected = true;
                    em.InfectionTimer = 0.1f;
                }
                MaxCd3 = 4f; return true; 
            }
            return false;
        }

        private bool UseBlue(Vector2 dir, List<EnemyModel> enemies)
        {
            if (ActiveSlot == 0 && ConsumeMana(5f)) {
                foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 100f && Vector2.Dot(dir, Vector2.Normalize(e.Position - Position)) > 0.5f) e.TakeDamage(15f, Position, 100f);
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

        private object GetClosestEntity(Vector2 aimDir, List<EnemyModel> enemies, NpcModel npc, float maxRange, float minDot) {
            object best = null; 
            float bestDot = minDot;
            
            foreach (var e in enemies) {
                Vector2 toE = e.Position - Position;
                if (toE.Length() > 0 && toE.Length() < maxRange) {
                    float dot = Vector2.Dot(aimDir, Vector2.Normalize(toE));
                    if (dot > bestDot) { bestDot = dot; best = e; }
                }
            } 
            
            if (npc != null && !npc.IsPickedUp) {
                Vector2 toN = npc.Position - Position;
                if (toN.Length() > 0 && toN.Length() < maxRange) {
                    float dot = Vector2.Dot(aimDir, Vector2.Normalize(toN));
                    if (dot > bestDot) { bestDot = dot; best = npc; }
                }
            }
            return best;
        }
    }
}
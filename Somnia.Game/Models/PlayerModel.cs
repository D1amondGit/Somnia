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
        
        // Надежный способ без массивов: 3 кулдауна для 3 слотов
        public float Cd1 { get; private set; }
        public float Cd2 { get; private set; }
        public float Cd3 { get; private set; }
        
        public float MaxCd1 { get; private set; }
        public float MaxCd2 { get; private set; }
        public float MaxCd3 { get; private set; }
        
        public bool IsDashing { get; private set; }

        private float _dashTimer, _dashCd;
        private Vector2 _dashDir;

        public PlayerModel(Vector2 start) => Position = start;
        public void SetState(PlayerState s) => State = s;
        public void TakeDamage(float a) => CurrentHealth = Math.Max(0, CurrentHealth - a);
        public void UpdateFacing(Vector2 d) { if (d != Vector2.Zero) { d.Normalize(); FacingDir = d; } }

        public void Move(Vector2 dir, float dt, int w, int h)
        {
            CurrentMana = Math.Min(100f, CurrentMana + 10f * dt);
            
            if (Cd1 > 0) Cd1 -= dt;
            if (Cd2 > 0) Cd2 -= dt;
            if (Cd3 > 0) Cd3 -= dt;
            
            if (_dashCd > 0) _dashCd -= dt;
            if (_dashTimer > 0) { _dashTimer -= dt; if (_dashTimer <= 0) IsDashing = false; }

            Vector2 move = IsDashing ? _dashDir * 2000f : (dir != Vector2.Zero ? Vector2.Normalize(dir) * (State == PlayerState.Carrying ? 250f : 500f) : Vector2.Zero);
            Position = new Vector2(MathHelper.Clamp(Position.X + move.X * dt, 0, w - 50), MathHelper.Clamp(Position.Y + move.Y * dt, 0, h - 50));
        }

        public void UseActiveSkill(Vector2 target, List<EnemyModel> enemies)
        {
            if (State == PlayerState.Carrying) return;
            
            if (ActiveSlot == 0 && Cd1 > 0) return;
            if (ActiveSlot == 1 && Cd2 > 0) return;
            if (ActiveSlot == 2 && Cd3 > 0) return;

            bool success = CurrentZone == AnomalyType.Red ? UseRed(target, enemies) : 
                           CurrentZone == AnomalyType.Blue ? UseBlue(target, enemies) : UseGreen(target, enemies);
            
            if (success) 
            {
                if (ActiveSlot == 0) Cd1 = MaxCd1;
                if (ActiveSlot == 1) Cd2 = MaxCd2;
                if (ActiveSlot == 2) Cd3 = MaxCd3;
            }
        }

        private bool UseRed(Vector2 dir, List<EnemyModel> enemies)
        {
            if (ActiveSlot == 0 && ConsumeMana(10f)) { PerformHit(dir, enemies, 180f, 35f, 900f); MaxCd1 = 0.5f; return true; }
            if (ActiveSlot == 1 && ConsumeMana(20f)) { RedLasso(enemies); MaxCd2 = 2f; return true; }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { RedNuke(dir, enemies); MaxCd3 = 5f; return true; }
            return false;
        }

        private bool UseGreen(Vector2 dir, List<EnemyModel> enemies)
        {
            if (ActiveSlot == 0 && ConsumeMana(15f)) { PerformAoE(Position + dir * 50, enemies, 120f, 25f, 0f); MaxCd1 = 1f; return true; }
            if (ActiveSlot == 1 && ConsumeMana(30f)) { PerformAoE(Position, enemies, 200f, 10f, 1500f); MaxCd2 = 3f; return true; } 
            if (ActiveSlot == 2 && ConsumeMana(40f)) { PerformAoE(Position + dir * 100, enemies, 250f, 40f, 0f); MaxCd3 = 4f; return true; } 
            return false;
        }

        private bool UseBlue(Vector2 dir, List<EnemyModel> enemies)
        {
            if (ActiveSlot == 0 && ConsumeMana(5f)) { PerformHit(dir, enemies, 100f, 15f, 100f); MaxCd1 = 0.2f; return true; }
            if (ActiveSlot == 1 && ConsumeMana(25f)) { BlueDashStun(dir, enemies); MaxCd2 = 3f; return true; }
            if (ActiveSlot == 2 && ConsumeMana(50f)) { foreach(var e in enemies) e.SlowTimer=4f; MaxCd3 = 10f; return true; }
            return false;
        }

        private bool ConsumeMana(float amount) { if (CurrentMana >= amount) { CurrentMana -= amount; return true; } return false; }
        
        private void PerformHit(Vector2 dir, List<EnemyModel> enemies, float range, float dmg, float kb)
        {
            if (dir != Vector2.Zero) dir.Normalize();
            foreach (var e in enemies) {
                Vector2 toE = e.Position - Position;
                if (toE.Length() < range && Vector2.Dot(dir, Vector2.Normalize(toE)) > 0.5f) e.TakeDamage(dmg, Position, kb);
            }
        }
        
        private void PerformAoE(Vector2 center, List<EnemyModel> enemies, float radius, float dmg, float kb) {
            foreach (var e in enemies) if (Vector2.Distance(center, e.Position) < radius) e.TakeDamage(dmg, center, kb);
        }
        
        private void RedLasso(List<EnemyModel> enemies) {
            foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 400f) e.Position = Vector2.Lerp(e.Position, Position, 0.7f);
        }
        
        private void RedNuke(Vector2 dir, List<EnemyModel> enemies) { PerformHit(dir, enemies, 150f, 200f, 0f); }
        
        private void BlueDashStun(Vector2 dir, List<EnemyModel> enemies) {
            if (dir != Vector2.Zero && !IsDashing && _dashCd <= 0) { dir.Normalize(); IsDashing = true; _dashTimer = 0.2f; _dashCd = 1.5f; _dashDir = dir; }
            foreach (var e in enemies) if (Vector2.Distance(Position, e.Position) < 200f) e.StunTimer = 2.5f;
        }
    }
}
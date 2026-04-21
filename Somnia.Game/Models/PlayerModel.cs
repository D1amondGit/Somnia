using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }
    public enum AnomalyType { Red, Blue, Green, Neutral }

    public class PlayerModel
    {
        public Vector2 Position;
        public Vector2 FacingDir = Vector2.UnitX;
        public float CurrentHealth = 100f, MaxHealth = 100f;
        public float CurrentMana = 100f, DamageMultiplier = 1.0f;
        public int ActiveSlot = 0;
        public AnomalyType CurrentZone;
        public PlayerState State = PlayerState.Free;
        
        public float Cd1, Cd2, Cd3;
        public float MaxCd1 = 0.5f, MaxCd2 = 2f, MaxCd3 = 5f;
        public float DashTimer, DashCooldown, GreenAuraTimer;
        public bool IsDashing => DashTimer > 0;
        public bool IsAttacking => _attackTimer > 0;
        public bool IsDead => CurrentHealth <= 0;
        private float _attackTimer;

        public PlayerModel(Vector2 pos) { Position = pos; }

        public void TakeDamage(float dmg) => CurrentHealth -= dmg;
        public void SetState(PlayerState s) => State = s;
        public void StartDash() { if(DashCooldown <= 0) { DashTimer = 0.2f; DashCooldown = 1f; } }
        public bool ConsumeMana(float amount) { if (CurrentMana >= amount) { CurrentMana -= amount; return true; } return false; }

        public void UseActiveSkill(Vector2 target, List<EnemyModel> enemies, NpcModel npc)
        {
            if (State == PlayerState.Carrying || (ActiveSlot == 0 && Cd1 > 0) || (ActiveSlot == 1 && Cd2 > 0) || (ActiveSlot == 2 && Cd3 > 0)) return;

            float m = (npc != null && !npc.IsDead && npc.Health < 50f) ? 0.5f : 1f;
            Vector2 dir = target != Vector2.Zero ? Vector2.Normalize(target) : Vector2.UnitX;

            bool s = CurrentZone switch {
                AnomalyType.Red => UseRed(dir, enemies, npc, m),
                AnomalyType.Blue => UseBlue(dir, enemies, m),
                AnomalyType.Green => UseGreen(dir, enemies, m),
                AnomalyType.Neutral => UseNeutral(dir, enemies, m),
                _ => false
            };
    
            if (s) { _attackTimer = 0.15f; if (ActiveSlot == 0) Cd1 = MaxCd1; else if (ActiveSlot == 1) Cd2 = MaxCd2; else Cd3 = MaxCd3; }
        }
        
        private bool UseRed(Vector2 dir, List<EnemyModel> e, NpcModel npc, float m) { return true; }
        private bool UseBlue(Vector2 dir, List<EnemyModel> e, float m) { return true; }
        private bool UseGreen(Vector2 dir, List<EnemyModel> e, float m) { return true; }
        private bool UseNeutral(Vector2 dir, List<EnemyModel> e, float m) {
            if (ActiveSlot == 0 && ConsumeMana(5f)) { MaxCd1 = 0.3f; return true; } return false;
        }

        public void Update(float dt)
        {
            if (DashTimer > 0) DashTimer -= dt;
            if (DashCooldown > 0) DashCooldown -= dt;
            if (Cd1 > 0) Cd1 -= dt; if (Cd2 > 0) Cd2 -= dt; if (Cd3 > 0) Cd3 -= dt;
            if (_attackTimer > 0) _attackTimer -= dt;
            CurrentMana = MathHelper.Min(100f, CurrentMana + 10f * dt);
        }
    }
}
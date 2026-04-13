using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public class EnemyModel
    {
        public Vector2 Position { get; set; }
        public float MaxHealth { get; set; } = 50f;
        public float CurrentHealth { get; set; }
        public float Speed { get; set; } = 150f;
        public float Damage { get; set; } = 10f;
        public float AttackRadius { get; set; } = 40f;
        public bool IsDead => CurrentHealth <= 0;

        private Vector2 _knockbackVelocity;
        private float _attackCooldown;
        private const float AttackCooldownTime = 1f;

        public EnemyModel(Vector2 startPosition)
        {
            Position = startPosition;
            CurrentHealth = MaxHealth;
            _knockbackVelocity = Vector2.Zero;
            _attackCooldown = 0f;
        }

        public void TakeDamage(float amount, Vector2 knockbackDirection)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            ApplyKnockback(knockbackDirection);
        }

        private void ApplyKnockback(Vector2 direction)
        {
            if (direction != Vector2.Zero)
            {
                _knockbackVelocity = Vector2.Normalize(direction) * 400f;
            }
        }

        public void Update(float deltaTime)
        {
            UpdateKnockback(deltaTime);
            UpdateAttackCooldown(deltaTime);
        }

        private void UpdateKnockback(float deltaTime)
        {
            if (_knockbackVelocity != Vector2.Zero)
            {
                Position += _knockbackVelocity * deltaTime;
                _knockbackVelocity *= 0.9f;
                if (_knockbackVelocity.Length() < 10f)
                {
                    _knockbackVelocity = Vector2.Zero;
                }
            }
        }

        private void UpdateAttackCooldown(float deltaTime)
        {
            if (_attackCooldown > 0)
            {
                _attackCooldown -= deltaTime;
            }
        }

        public bool CanAttack() => _attackCooldown <= 0;

        public void PerformAttack() => _attackCooldown = AttackCooldownTime;
    }
}

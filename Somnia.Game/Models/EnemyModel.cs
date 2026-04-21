using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public class EnemyModel
    {
        public Vector2 Position { get; set; }
        public float Health { get; set; } = 60f;
        public float MaxHealth { get; set; } = 60f;
        public float Speed { get; set; } = 150f;
        public float AttackRadius { get; set; } = 50f;
        public float Damage { get; set; } = 10f;
        public bool IsDead => Health <= 0;
        public bool HasDropped { get; set; }

        private Vector2 _velocity;
        private float _attackCooldown;

        public EnemyModel(Vector2 start) => Position = start;

        public bool CanAttack() => _attackCooldown <= 0;
        public void PerformAttack() => _attackCooldown = 1.0f;

        public void TakeDamage(float dmg, Vector2 source, float kbPower)
        {
            Health -= dmg;
            if (kbPower > 0) _velocity = Vector2.Normalize(Position - source) * kbPower;
        }

        public void Update(float dt)
        {
            Position += _velocity * dt;
            _velocity = Vector2.Lerp(_velocity, Vector2.Zero, 0.1f);
            if (_attackCooldown > 0) _attackCooldown -= dt;
        }
    }
}

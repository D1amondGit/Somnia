using System;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class EnemyModel
    {
        public Vector2 Position { get; set; }
        public float Health { get; set; } = 60f;
        public float MaxHealth { get; set; } = 60f;
        public bool IsDead => Health <= 0;
        public float AttackCooldown { get; set; }
        
        public float StunTimer { get; set; }
        public float SlowTimer { get; set; }
        private Vector2 _velocity;

        public EnemyModel(Vector2 start) => Position = start;

        public void TakeDamage(float dmg, Vector2 source, float kbPower)
        {
            Health -= dmg;
            if (kbPower > 0 && Position != source)
                _velocity = Vector2.Normalize(Position - source) * kbPower;
        }

        public void Update(float dt)
        {
            if (StunTimer > 0) StunTimer -= dt;
            if (SlowTimer > 0) SlowTimer -= dt;
            
            Position += _velocity * dt;
            _velocity = Vector2.Lerp(_velocity, Vector2.Zero, 0.1f);
        }
    }
}
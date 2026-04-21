using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public class NpcModel
    {
        public Vector2 Position { get; set; }
        public float Health { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public bool IsPickedUp { get; set; }
        public bool IsDead => Health <= 0;
        public bool IsInjured => Health / MaxHealth < 0.5f;

        public NpcModel(Vector2 start) => Position = start;

        public void TakeDamage(float amt) => Health = Math.Max(0, Health - amt);
    }
}

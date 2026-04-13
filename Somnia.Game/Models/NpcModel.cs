using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class NpcModel
    {
        public Vector2 Position { get; set; }
        public bool IsPickedUp { get; set; }
        public float Health { get; set; } = 100f;
        public bool IsDead => Health <= 0;

        public NpcModel(Vector2 startPos)
        {
            Position = startPos;
        }

        public void TakeDamage(float dmg)
        {
            Health -= dmg;
        }
    }
}
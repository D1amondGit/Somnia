using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class ProjectileModel
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Damage { get; set; }

        public ProjectileModel(Vector2 pos, Vector2 vel, float dmg)
        {
            Position = pos;
            Velocity = vel;
            Damage = dmg;
        }

        public void Update(float dt) => Position += Velocity * dt;
    }
}
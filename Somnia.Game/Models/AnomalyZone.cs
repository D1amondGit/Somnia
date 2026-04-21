using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class AnomalyZone
    {
        public Rectangle Bounds { get; }
        public AnomalyType Type { get; }

        public AnomalyZone(Rectangle bounds, AnomalyType type)
        {
            Bounds = bounds;
            Type = type;
        }

        public bool ContainsPoint(System.Numerics.Vector2 point)
            => Bounds.Contains((int)point.X, (int)point.Y);
    }
}
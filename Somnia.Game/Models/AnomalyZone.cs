using Microsoft.Xna.Framework;
using System.Numerics;

namespace Somnia.Game.Models
{
    public enum ZoneType { Neutral, Red, Green, Blue }

    public class AnomalyZone
    {
        public Rectangle Bounds { get; }
        public ZoneType Type { get; }

        public AnomalyZone(Rectangle bounds, ZoneType type)
        {
            Bounds = bounds;
            Type = type;
        }

        public bool ContainsPoint(System.Numerics.Vector2 point)
        {
            return Bounds.Contains((int)point.X, (int)point.Y);
        }
    }
}

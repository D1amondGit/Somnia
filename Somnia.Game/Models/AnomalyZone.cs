using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class AnomalyZone
    {
        public Rectangle Area;
        public AnomalyType Type;

        public AnomalyZone(Rectangle r, AnomalyType t)
        {
            Area = r;
            Type = t;
        }

        public bool ContainsPoint(Vector2 point)
        {
            return Area.Contains((int)point.X, (int)point.Y);
        }
    }
}
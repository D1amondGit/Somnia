using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class AnomalyZone
    {
        public Vector2 Center;
        public float Radius;
        public AnomalyType Type;

        public AnomalyZone(Vector2 c, float r, AnomalyType t)
        {
            Center = c; Radius = r; Type = t;
        }

        public bool ContainsPoint(Vector2 p)
        {
            // Корректируем радиус под изометрию (Y сжат на 0.7f)
            Vector2 diff = p - Center;
            diff.Y /= 0.7f; 
            return diff.Length() <= Radius;
        }
    }
}
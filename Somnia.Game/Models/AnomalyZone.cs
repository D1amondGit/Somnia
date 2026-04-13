using Microsoft.Xna.Framework;

namespace Somnia.Game.Models
{
    public class AnomalyZone
    {
        public Rectangle Area { get; set; }
        public AnomalyType Type { get; set; }

        public AnomalyZone(Rectangle area, AnomalyType type)
        {
            Area = area;
            Type = type;
        }
    }
}
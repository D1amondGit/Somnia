using Microsoft.Xna.Framework;
using System.Numerics;

namespace Somnia.Game.Models
{
    public enum ZoneType { Neutral, Red, Blue }

    public class AnomalyZone
    {
        public Rectangle Bounds { get; }
        public ZoneType Type { get; }
        public Color ZoneColor { get; }
        public float AttackDamage { get; }
        public float AttackCooldown { get; }
        public float AttackRange { get; }

        public AnomalyZone(Rectangle bounds, ZoneType type)
        {
            Bounds = bounds;
            Type = type;
            (AttackDamage, AttackCooldown, AttackRange) = GetZoneStats(type);
            ZoneColor = GetZoneVisualColor(type);
        }

        private static (float damage, float cooldown, float range) GetZoneStats(ZoneType type)
        {
            return type switch
            {
                ZoneType.Red => (25f, 1.2f, 80f),
                ZoneType.Blue => (8f, 0.25f, 60f),
                _ => (15f, 0.6f, 50f),
            };
        }

        private static Color GetZoneVisualColor(ZoneType type)
        {
            return type switch
            {
                ZoneType.Red => Color.Red,
                ZoneType.Blue => Color.Blue,
                _ => Color.White,
            };
        }

        public bool ContainsPoint(System.Numerics.Vector2 point)
        {
            return Bounds.Contains((int)point.X, (int)point.Y);
        }
    }
}

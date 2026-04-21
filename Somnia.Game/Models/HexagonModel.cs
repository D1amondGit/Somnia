using Microsoft.Xna.Framework;
using System;

namespace Somnia.Game.Models
{
    public class HexagonModel
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public float WallHeight { get; set; }

        public HexagonModel(Vector2 center, float radius, float height = 30f)
        {
            Center = center;
            Radius = radius;
            WallHeight = height;
        }

        public Vector2[] GetVertices()
        {
            Vector2[] v = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(i * 60 - 30);
                v[i] = Center + new Vector2(
                    (float)Math.Cos(angle) * Radius,
                    (float)Math.Sin(angle) * Radius);
            }
            return v;
        }
    }
}
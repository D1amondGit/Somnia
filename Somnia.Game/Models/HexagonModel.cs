using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public class HexagonModel
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public float WallHeight { get; set; }
        public float Squash { get; set; }
        public float Tilt { get; set; }

        public HexagonModel(Vector2 center, float radius, float h = 50f, float sq = 0.75f, float t = 0.1f)
        {
            Center = center; Radius = radius; WallHeight = h; Squash = sq; Tilt = t;
        }

        public List<Vector2> GetTopVertices() => GetVerticesAt(WallHeight);
        public List<Vector2> GetBaseVertices() => GetVerticesAt(0);

        private List<Vector2> GetVerticesAt(float z)
        {
            var v = new List<Vector2>();
            for (int i = 0; i < 6; i++)
            {
                float a = MathHelper.ToRadians(i * 60); 
                float x = (float)Math.Cos(a) * Radius;
                float baseYa = (float)Math.Sin(a) * Radius;
                // Применяем новые параметры
                float y = (baseYa * Squash) - (x * Tilt);
                v.Add(Center + new Vector2(x, y - z));
            }
            return v;
        }
    }
}
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
        public float SquashFactor { get; set; }

        public HexagonModel(Vector2 center, float radius, float height = 50f, float squash = 0.5f)
        {
            Center = center;
            Radius = radius;
            WallHeight = height;
            SquashFactor = squash;
        }

        public List<Vector2> GetTopVertices() => GetVerticesAt(WallHeight);
        public List<Vector2> GetBaseVertices() => GetVerticesAt(0);

        private List<Vector2> GetVerticesAt(float zOffset)
        {
            var list = new List<Vector2>();
            for (int i = 0; i < 6; i++)
            {
                // Угол поворота: i * 60 - 30 градусов, чтобы гексагон стоял на острие (Pointy-topped)
                float angle = MathHelper.ToRadians(i * 60 - 30); 
                float x = (float)Math.Cos(angle) * Radius;
                float y = (float)Math.Sin(angle) * Radius * SquashFactor;
                list.Add(Center + new Vector2(x, y - zOffset));
            }
            return list;
        }
    }
}
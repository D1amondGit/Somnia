using Microsoft.Xna.Framework;
using System;

namespace Somnia.Game.Models
{
    public static class PhysicsHelper
    {
        public static void ResolveHexCollision(ref Vector2 pos, float radius, HexagonModel hex)
        {
            Vector2 localP = pos - hex.Center;
            float apothem = hex.Radius * 0.866f; // Расстояние до плоской грани
            
            // Быстрая проверка по радиусу
            if (localP.LengthSquared() > Math.Pow(hex.Radius + radius, 2)) return;

            Vector2[] normals = GetHexNormals();
            float maxDist = -1f;
            Vector2 bestNormal = Vector2.Zero;

            foreach (var n in normals)
            {
                float d = Vector2.Dot(localP, n);
                if (d > maxDist) { maxDist = d; bestNormal = n; }
            }

            float penetration = (apothem + radius) - maxDist;
            if (penetration > 0) pos += bestNormal * penetration;
        }

        private static Vector2[] GetHexNormals()
        {
            float s32 = 0.866f;
            return new Vector2[] { 
                new(0, 1), new(0, -1), 
                new(s32, 0.5f), new(-s32, -0.5f), 
                new(s32, -0.5f), new(-s32, 0.5f) 
            };
        }
    }
}
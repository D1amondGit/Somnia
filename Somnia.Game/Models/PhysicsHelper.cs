using Microsoft.Xna.Framework;
using System;

namespace Somnia.Game.Models
{
    public static class PhysicsHelper
    {
        public static void ResolveHexCollision(ref Vector2 pos, float radius, HexagonModel hex)
        {
            Vector2 localP = pos - hex.Center;
            float apothem = hex.Radius * 0.8660254f; 
            
            if (localP.LengthSquared() > (hex.Radius + radius) * (hex.Radius + radius)) return;

            float s32 = 0.8660254f;
            Vector2[] normals = new Vector2[] {
                new(0, 1), new(0, -1), new(s32, 0.5f), new(-s32, -0.5f), new(s32, -0.5f), new(-s32, 0.5f)
            };
            
            float maxDist = -9999f;
            Vector2 bestN = Vector2.Zero;
            
            foreach (var n in normals) {
                float d = Vector2.Dot(localP, n) - apothem;
                if (d > maxDist) { maxDist = d; bestN = n; }
            }
            
            if (maxDist < radius) pos += bestN * (radius - maxDist);
        }
    }
}
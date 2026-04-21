using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public static class PhysicsHelper
    {
        public static void ResolveHexCollision(ref Vector2 pos, float radius, HexagonModel hex)
        {
            Vector2 roofC = hex.Center - new Vector2(0, hex.WallHeight);
            Vector2 lp = pos - roofC;
            
            // ОБРАТНАЯ ТРАНСФОРМАЦИЯ: превращаем наклонный овал обратно в ровный круг
            float untransformedY = (lp.Y + lp.X * hex.Tilt) / hex.Squash;
            Vector2 pClean = new Vector2(lp.X, untransformedY);
            
            float s32 = 0.8660254f; float apothem = hex.Radius * s32;
            if (pClean.LengthSquared() > Math.Pow(hex.Radius + radius, 2)) return;

            var ns = new List<Vector2>(); // Нормали Flat-topped
            ns.Add(new(1, 0)); ns.Add(new(0.5f, s32)); ns.Add(new(-0.5f, s32));
            ns.Add(new(-1, 0)); ns.Add(new(-0.5f, -s32)); ns.Add(new(0.5f, -s32));
            
            float maxD = -9999f; Vector2 bestN = Vector2.UnitX;
            foreach (var n in ns) {
                float d = Vector2.Dot(pClean, n) - apothem;
                if (d > maxD) { maxD = d; bestN = n; }
            }
            
            if (maxD < radius) {
                Vector2 push = bestN * (radius - maxD);
                // Сжимаем вектор выталкивания обратно под угол
                pos += new Vector2(push.X, (push.Y * hex.Squash) - (push.X * hex.Tilt));
            }
        }
    }
}
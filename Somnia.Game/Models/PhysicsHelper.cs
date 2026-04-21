using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public static class PhysicsHelper
    {
        public static void ResolveHexCollision(ref Vector2 pos, float radius, HexagonModel hex)
        {
            // 1. "Отменяем" сплющивание для вычислений, чтобы коллизия была идеально ровной
            Vector2 localP = pos - (hex.Center - new Vector2(0, hex.WallHeight)); // Коллизия по крыше
            localP.Y /= hex.SquashFactor; 
            
            float s32 = 0.8660254f; // sin(60)
            float apothem = hex.Radius * s32; 
            
            // Быстрая проверка: если далеко, не считаем
            if (localP.LengthSquared() > (hex.Radius + radius) * (hex.Radius + radius)) return;

            // --- НОВЫЕ НОРМАЛИ ДЛЯ Pointy-topped ГЕКСАГОНА ---
            // Углы нормалей: 30, 90, 150, 210, 270, 330 градусов
            var normals = new List<Vector2>();
            normals.Add(new Vector2(s32, 0.5f));    // 30 deg
            normals.Add(new Vector2(0, 1));        // 90 deg
            normals.Add(new Vector2(-s32, 0.5f));   // 150 deg
            normals.Add(new Vector2(-s32, -0.5f));  // 210 deg
            normals.Add(new Vector2(0, -1));       // 270 deg
            normals.Add(new Vector2(s32, -0.5f));   // 330 deg
            
            float maxDist = -9999f;
            Vector2 bestN = Vector2.Zero;
            
            foreach (var n in normals) {
                float d = Vector2.Dot(localP, n) - apothem;
                if (d > maxDist) { maxDist = d; bestN = n; }
            }
            
            // 2. Выталкиваем и "сплющиваем" вектор отдачи обратно
            if (maxDist < radius) {
                Vector2 push = bestN * (radius - maxDist);
                push.Y *= hex.SquashFactor;
                pos += push;
            }
        }
    }
}
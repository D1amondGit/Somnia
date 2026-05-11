using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>Проверка прямой видимости с учётом изометрического масштаба стенок.</summary>
public interface ILineOfSightService
{
    bool HasLineOfSight(Vector2 from, Vector2 to, IReadOnlyList<HexagonModel> walls);
}

public sealed class LineOfSightService : ILineOfSightService
{
    public bool HasLineOfSight(Vector2 from, Vector2 to, IReadOnlyList<HexagonModel> walls)
    {
        foreach (var w in walls)
        {
            Vector2 c = new(w.Center.X, w.Center.Y);
            Vector2 ap = c - from;
            Vector2 ab = to - from;
            float ab2 = ab.LengthSquared();
            if (ab2 == 0f) continue;

            float t = MathHelper.Clamp(Vector2.Dot(ap, ab) / ab2, 0f, 1f);
            Vector2 diff = (from + ab * t) - c;
            diff.Y /= IsometricView.Squash;
            if (diff.Length() < w.Radius) return false;
        }

        return true;
    }
}

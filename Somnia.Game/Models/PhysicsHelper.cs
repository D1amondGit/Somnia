using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>
/// Коллизии с гекс-препятствиями в той же плоскости, что и отрисовка: «крышка» — это
/// <see cref="HexagonModel.GetTopVertices"/> (чёрный шестиугольник), без старых аппроксимаций по кругу/apothem.
/// </summary>
public static class PhysicsHelper
{
    private const float Epsilon = 1e-4f;

    /// <summary>Отрезок пересекает залитый многоугольник (в т.ч. концы внутри) — для LOS сквозь стену.</summary>
    public static bool SegmentIntersectsPolygon(Vector2 a, Vector2 b, IReadOnlyList<Vector2> poly)
    {
        if (poly.Count < 3) return false;
        if (PointInPolygon(a, poly) || PointInPolygon(b, poly))
            return true;

        var n = poly.Count;
        for (var i = 0; i < n; i++)
        {
            var p1 = poly[i];
            var p2 = poly[(i + 1) % n];
            if (SegmentsIntersect(a, b, p1, p2))
                return true;
        }

        return false;
    }

    /// <summary>Круг (центр <paramref name="pos"/>, радиус) vs выпуклый многоугольник крышки гекса.</summary>
    public static void ResolveHexCollision(ref Vector2 pos, float circleRadius, HexagonModel hex)
    {
        var poly = hex.GetTopVertices();
        if (poly.Count < 3) return;

        if (!CircleRoughlyOverlapsAabb(pos, circleRadius, poly))
            return;

        var closest = ClosestPointOnPolygonBoundary(pos, poly, out var dSq);
        var inside = PointInPolygon(pos, poly);
        var rSq = circleRadius * circleRadius;

        if (!inside)
        {
            if (dSq >= rSq) return;
            var d = MathF.Sqrt(dSq);
            if (d < Epsilon) return;
            pos += (pos - closest) * ((circleRadius - d) / d);
            return;
        }

        // Внутри полигона: выталкиваем центр круга наружу на skin от ближайшей точки на границе.
        var inward = pos - closest;
        var len = inward.Length();
        if (len > Epsilon)
            pos = closest - inward * (circleRadius / len);
        else
        {
            var c = PolygonCentroid(poly);
            var away = pos - c;
            if (away.LengthSquared() < Epsilon * Epsilon)
                away = new Vector2(1f, 0f);
            else
                away.Normalize();
            pos = closest - away * circleRadius;
        }
    }

    private static bool CircleRoughlyOverlapsAabb(Vector2 pos, float r, IReadOnlyList<Vector2> poly)
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        foreach (var v in poly)
        {
            if (v.X < minX) minX = v.X;
            if (v.X > maxX) maxX = v.X;
            if (v.Y < minY) minY = v.Y;
            if (v.Y > maxY) maxY = v.Y;
        }

        return !(pos.X + r < minX || pos.X - r > maxX || pos.Y + r < minY || pos.Y - r > maxY);
    }

    private static Vector2 PolygonCentroid(IReadOnlyList<Vector2> poly)
    {
        var s = Vector2.Zero;
        foreach (var v in poly)
            s += v;
        return s / poly.Count;
    }

    private static Vector2 ClosestPointOnPolygonBoundary(Vector2 p, IReadOnlyList<Vector2> poly, out float distSq)
    {
        distSq = float.MaxValue;
        var best = poly[0];
        var n = poly.Count;
        for (var i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            var cp = ClosestPointOnSegment(a, b, p);
            var ds = Vector2.DistanceSquared(p, cp);
            if (ds < distSq)
            {
                distSq = ds;
                best = cp;
            }
        }

        return best;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        var ab = b - a;
        var ab2 = ab.LengthSquared();
        if (ab2 < Epsilon * Epsilon)
            return a;
        var t = MathHelper.Clamp(Vector2.Dot(p - a, ab) / ab2, 0f, 1f);
        return a + ab * t;
    }

    /// <summary>Ray-cast (чётность пересечений), устойчиво для невыпуклых; для гекса достаточно.</summary>
    public static bool PointInPolygon(Vector2 p, IReadOnlyList<Vector2> poly)
    {
        var n = poly.Count;
        if (n < 3) return false;
        var inside = false;
        for (var i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            if (Math.Abs(a.Y - b.Y) < Epsilon) continue;

            if ((a.Y > p.Y) == (b.Y > p.Y)) continue;

            var xInt = (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X;
            if (p.X < xInt)
                inside = !inside;
        }

        return inside;
    }

    private static bool SegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        static float Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X;

        var d1 = Cross(a2 - a1, b1 - a1);
        var d2 = Cross(a2 - a1, b2 - a1);
        var d3 = Cross(b2 - b1, a1 - b1);
        var d4 = Cross(b2 - b1, a2 - b1);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        if (Math.Abs(d1) < Epsilon && OnSegment(a1, a2, b1)) return true;
        if (Math.Abs(d2) < Epsilon && OnSegment(a1, a2, b2)) return true;
        if (Math.Abs(d3) < Epsilon && OnSegment(b1, b2, a1)) return true;
        if (Math.Abs(d4) < Epsilon && OnSegment(b1, b2, a2)) return true;

        return false;
    }

    private static bool OnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        return p.X <= MathF.Max(a.X, b.X) + Epsilon && p.X + Epsilon >= MathF.Min(a.X, b.X) &&
               p.Y <= MathF.Max(a.Y, b.Y) + Epsilon && p.Y + Epsilon >= MathF.Min(a.Y, b.Y);
    }
}

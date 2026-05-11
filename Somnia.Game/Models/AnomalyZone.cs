using System;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>
/// Зона аномалии. Хранит произвольный замкнутый полигон (Outline), что позволяет
/// иметь не только эллиптическую/гексагональную, но и любую органичную форму.
/// </summary>
public class AnomalyZone
{
    public Vector2 Center;
    public float Radius;
    public AnomalyType Type;
    public Vector2[] Outline { get; }

    public AnomalyZone(Vector2 center, float radius, AnomalyType type)
        : this(center, radius, type, BuildDefaultEllipse(center, radius))
    {
    }

    public AnomalyZone(Vector2 center, float radius, AnomalyType type, Vector2[] outline)
    {
        Center = center;
        Radius = radius;
        Type = type;
        Outline = outline.Length >= 3 ? outline : BuildDefaultEllipse(center, radius);
    }

    /// <summary>Crossing-number алгоритм PiP. Корректно работает и для невыпуклых полигонов.</summary>
    public bool ContainsPoint(Vector2 p)
    {
        var inside = false;
        var n = Outline.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var a = Outline[i];
            var b = Outline[j];
            if (a.Y > p.Y == b.Y > p.Y) continue;
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            if (Math.Abs(dy) < 1e-6f) continue;
            var xIntersect = (p.Y - a.Y) * dx / dy + a.X;
            if (p.X < xIntersect) inside = !inside;
        }

        return inside;
    }

    private static Vector2[] BuildDefaultEllipse(Vector2 center, float radius, int segments = 36)
    {
        var pts = new Vector2[segments];
        for (var i = 0; i < segments; i++)
        {
            var a = i / (float)segments * MathHelper.TwoPi;
            pts[i] = center + new Vector2(MathF.Cos(a) * radius, MathF.Sin(a) * radius * IsometricView.Squash);
        }

        return pts;
    }
}

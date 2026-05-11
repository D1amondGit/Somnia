using System;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>Общая плоская гексагональная сетка (крупные ячейки) для пола и раскладки арены.</summary>
public static class ArenaHexGrid
{
    public const float CircumRadius = 230f;
    public const float Squash = IsometricView.Squash;
    public const float Tilt = IsometricView.Tilt;

    private static readonly float Sqrt3 = MathF.Sqrt(3f);

    public static float HorizontalSpacing => Sqrt3 * CircumRadius;

    public static float VerticalSpacing => 1.5f * CircumRadius;

    public static Vector2 GetOrigin(int seed)
    {
        var mixedSeed = seed ^ unchecked((int)0x9e3779b9);
        var rnd = new Random(mixedSeed);
        var h = HorizontalSpacing;
        var v = VerticalSpacing;
        return new Vector2(rnd.NextSingle() * h - h * 0.35f, rnd.NextSingle() * v - v * 0.35f);
    }

    public static Vector2 CellCenter(int q, int r, Vector2 origin)
    {
        var x = origin.X + HorizontalSpacing * (q + r * 0.5f);
        var y = origin.Y + VerticalSpacing * r;
        return new Vector2(x, y);
    }

    public static (int Q, int R) WorldToAxial(Vector2 world, Vector2 origin)
    {
        var relX = world.X - origin.X;
        var relY = world.Y - origin.Y;
        var fr = 2f / 3f * relY / CircumRadius;
        var fq = relX / CircumRadius / Sqrt3 - fr * 0.5f;
        return RoundAxial(fq, fr);
    }

    public static Vector2 SnapWorld(Vector2 world, Vector2 origin)
    {
        var (q, r) = WorldToAxial(world, origin);
        return CellCenter(q, r, origin);
    }

    private static (int Q, int R) RoundAxial(float q, float r)
    {
        var s = -q - r;
        var rq = (int)MathF.Round(q);
        var rr = (int)MathF.Round(r);
        var rs = (int)MathF.Round(s);
        var qDiff = Math.Abs(rq - q);
        var rDiff = Math.Abs(rr - r);
        var sDiff = Math.Abs(rs - s);
        if (qDiff > rDiff && qDiff > sDiff) rq = -rr - rs;
        else if (rDiff > sDiff) rr = -rq - rs;
        return (rq, rr);
    }
}

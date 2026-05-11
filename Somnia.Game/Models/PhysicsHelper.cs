using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public static class PhysicsHelper
{
    private const float Cos30 = 0.8660254f;

    public static void ResolveHexCollision(ref Vector2 pos, float playerRadius, HexagonModel hex)
    {
        var roofCenter = hex.Center - new Vector2(0, hex.WallHeight);
        var local = pos - roofCenter;

        var untransformedY = (local.Y + local.X * hex.Tilt) / hex.Squash;
        var clean = new Vector2(local.X, untransformedY);

        var apothem = hex.Radius * Cos30;
        if (clean.LengthSquared() > Math.Pow(hex.Radius + playerRadius, 2)) return;

        var normals = new List<Vector2>
        {
            new(1, 0),
            new(0.5f, Cos30),
            new(-0.5f, Cos30),
            new(-1, 0),
            new(-0.5f, -Cos30),
            new(0.5f, -Cos30)
        };

        var maxD = -9999f;
        var bestN = Vector2.UnitX;
        foreach (var n in normals)
        {
            var d = Vector2.Dot(clean, n) - apothem;
            if (d <= maxD) continue;
            maxD = d;
            bestN = n;
        }

        if (!(maxD < playerRadius)) return;

        var push = bestN * (playerRadius - maxD);
        pos += new Vector2(push.X, push.Y * hex.Squash - push.X * hex.Tilt);
    }
}

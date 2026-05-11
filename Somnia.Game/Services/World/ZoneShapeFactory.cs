using System;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>
/// Генерация органичных полигонов для зон аномалий. Использует мульти-октавный jitter,
/// чтобы получить «амёбоподобные» формы вместо правильных окружностей/гексов.
/// </summary>
public static class ZoneShapeFactory
{
    public static Vector2[] BuildOrganicOutline(Vector2 center, float radius, Random rnd,
        float ySquash = IsometricView.Squash)
    {
        var n = rnd.Next(13, 21);
        var pts = new Vector2[n];

        var phase1 = (float)rnd.NextDouble() * MathHelper.TwoPi;
        var phase2 = (float)rnd.NextDouble() * MathHelper.TwoPi;
        var phase3 = (float)rnd.NextDouble() * MathHelper.TwoPi;

        const float amp1 = 0.28f;
        const float amp2 = 0.16f;
        const float amp3 = 0.08f;

        for (var i = 0; i < n; i++)
        {
            var theta = i / (float)n * MathHelper.TwoPi;

            var jitter = 1f +
                         amp1 * MathF.Sin(theta * 2f + phase1) +
                         amp2 * MathF.Sin(theta * 3f + phase2) +
                         amp3 * MathF.Sin(theta * 5f + phase3);

            jitter = MathHelper.Clamp(jitter, 0.55f, 1.45f);

            var r = radius * jitter;
            var x = MathF.Cos(theta) * r;
            var y = MathF.Sin(theta) * r * ySquash;
            pts[i] = center + new Vector2(x, y);
        }

        return pts;
    }
}

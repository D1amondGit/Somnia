using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;

namespace Somnia.Game.Services.Particles;

/// <summary>
/// Эмиссия «искр» на верхушках стен — лёгкое мерцание для оживления статичной геометрии.
/// </summary>
public sealed class WallSparkleEmitter
{
    private readonly Random _rng;
    private float _accumulator;

    public WallSparkleEmitter(Random? rng = null) => _rng = rng ?? new Random();

    /// <summary>Частота эмиссии: одна искра на интервал на стену (амортизованно).</summary>
    public float EmitIntervalSeconds { get; set; } = 0.08f;

    public void Tick(List<WallSparkle> sparkles, IReadOnlyList<HexagonModel> walls, float dt)
    {
        for (var i = sparkles.Count - 1; i >= 0; i--)
        {
            sparkles[i].Lifetime -= dt;
            if (sparkles[i].Lifetime <= 0) sparkles.RemoveAt(i);
        }

        _accumulator += dt;
        if (_accumulator < EmitIntervalSeconds) return;

        _accumulator = 0f;
        var perBurst = Math.Max(2, walls.Count / 20);
        for (var i = 0; i < perBurst; i++)
        {
            if (walls.Count == 0) return;
            var w = walls[_rng.Next(walls.Count)];
            var topY = w.Center.Y - w.WallHeight;
            var a = (float)(_rng.NextDouble() * Math.PI * 2);
            var rr = w.Radius * (0.7f + (float)_rng.NextDouble() * 0.3f);
            var pos = new Vector2(
                w.Center.X + (float)Math.Cos(a) * rr,
                topY + (float)Math.Sin(a) * rr * IsometricView.Squash);

            sparkles.Add(new WallSparkle
            {
                Position = pos,
                Lifetime = 0.35f + (float)_rng.NextDouble() * 0.3f,
                MaxLifetime = 0.55f,
                Size = 1.5f + (float)_rng.NextDouble() * 1.5f,
                Color = new Color(220, 230, 255)
            });
        }
    }
}

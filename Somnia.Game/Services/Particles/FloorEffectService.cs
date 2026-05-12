using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models.Particles;

namespace Somnia.Game.Services.Particles;

/// <summary>
/// Эмиссия и тик «брызг» на полу: попадание пули, кровь врага, ошмётки взрыва.
/// </summary>
public sealed class FloorEffectService
{
    private readonly Random _rng;

    public FloorEffectService(Random? rng = null) => _rng = rng ?? new Random();

    public void Tick(List<FloorSplatter> splatters, float dt)
    {
        for (var i = splatters.Count - 1; i >= 0; i--)
        {
            var s = splatters[i];
            s.Lifetime -= dt;
            if (s.IsScorch)
            {
                s.Velocity *= MathHelper.Clamp(1f - 1.5f * dt, 0f, 1f);
            }
            else
            {
                s.Position += s.Velocity * dt;
                s.Velocity *= MathHelper.Clamp(1f - 4f * dt, 0f, 1f);
            }

            if (s.Lifetime <= 0f) splatters.RemoveAt(i);
        }
    }

    /// <summary>Брызги крови/шрапнели при попадании во врага или игрока.
    /// Размеры подобраны так, чтобы быть хорошо видимыми на тёмном поле даже после изо-сжатия.</summary>
    public void EmitImpact(List<FloorSplatter> splatters, Vector2 at, Color tint, int count = 8, float spread = 160f)
    {
        for (var i = 0; i < count; i++)
        {
            var angle = (float)(_rng.NextDouble() * Math.PI * 2);
            var speed = (float)(_rng.NextDouble() * spread + spread * 0.3f);
            splatters.Add(new FloorSplatter
            {
                Position = at,
                Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                Radius = 5f + (float)_rng.NextDouble() * 5f,
                Lifetime = 0.8f + (float)_rng.NextDouble() * 0.5f,
                MaxLifetime = 1.3f,
                Color = tint
            });
        }
    }

    /// <summary>Гарь и копоть, остаются дольше — для взрывов ракет/гранат.</summary>
    public void EmitScorch(List<FloorSplatter> splatters, Vector2 at, float radius, Color tint, int count = 22)
    {
        for (var i = 0; i < count; i++)
        {
            var angle = (float)(_rng.NextDouble() * Math.PI * 2);
            var r = (float)(_rng.NextDouble() * radius);
            splatters.Add(new FloorSplatter
            {
                Position = at + new Vector2((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r),
                Velocity = Vector2.Zero,
                Radius = 6f + (float)_rng.NextDouble() * 6f,
                Lifetime = 2.4f + (float)_rng.NextDouble() * 1.0f,
                MaxLifetime = 3.4f,
                Color = tint,
                IsScorch = true
            });
        }
    }
}

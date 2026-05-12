using System;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Camera;

/// <summary>
/// Тряска камеры в стиле Hotline Miami: «травма» накапливается от событий и плавно затухает,
/// смещение — функция (trauma^2). Это даёт мягкое начало и резкий пик при крупных событиях.
/// </summary>
public sealed class CameraShakeService
{
    private readonly Random _rng;
    private const float MaxOffset = 15f;
    private const float DecayPerSecond = 1.45f;

    public CameraShakeService(Random? rng = null) => _rng = rng ?? new Random();

    public void Trigger(CameraState camera, float amount)
    {
        camera.ShakeTrauma = MathHelper.Clamp(camera.ShakeTrauma + amount, 0f, 1f);
    }

    public void Tick(CameraState camera, float dt)
    {
        if (camera.ShakeTrauma <= 0f)
        {
            camera.ShakeOffset = Vector2.Zero;
            return;
        }

        var t = camera.ShakeTrauma;
        var mag = t * t * MaxOffset;

        camera.ShakeOffset = new Vector2(
            (float)(_rng.NextDouble() * 2 - 1) * mag,
            (float)(_rng.NextDouble() * 2 - 1) * mag);

        camera.ShakeTrauma = Math.Max(0f, camera.ShakeTrauma - DecayPerSecond * dt);
    }
}

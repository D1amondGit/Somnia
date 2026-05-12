using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.Particles;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class WallSparkleEmitterTests
{
    [Test]
    public void Tick_EmitsSparklesOverTime()
    {
        var emitter = new WallSparkleEmitter(new Random(1));
        var walls = new List<HexagonModel>
        {
            new(new Vector2(100, 100), 60f, 50f),
            new(new Vector2(200, 200), 60f, 50f),
            new(new Vector2(300, 300), 60f, 50f),
            new(new Vector2(400, 400), 60f, 50f),
        };
        var list = new List<WallSparkle>();

        for (var i = 0; i < 10; i++)
            emitter.Tick(list, walls, 0.1f);

        Assert.That(list.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Tick_RemovesExpiredSparkles()
    {
        var emitter = new WallSparkleEmitter(new Random(2)) { EmitIntervalSeconds = 100f };
        var walls = new List<HexagonModel> { new(Vector2.Zero, 60f, 50f) };
        var expired = new WallSparkle { Lifetime = 0.05f, MaxLifetime = 0.5f };
        var list = new List<WallSparkle> { expired };

        emitter.Tick(list, walls, dt: 0.5f);

        Assert.That(list, Does.Not.Contain(expired));
    }
}

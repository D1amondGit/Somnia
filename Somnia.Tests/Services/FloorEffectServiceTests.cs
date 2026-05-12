using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.Particles;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class FloorEffectServiceTests
{
    private static FloorEffectService NewFx(int seed = 12345) => new(new Random(seed));

    [Test]
    public void EmitImpact_AddsRequestedCount()
    {
        var list = new List<FloorSplatter>();
        var fx = NewFx();
        fx.EmitImpact(list, Vector2.Zero, Color.Red, count: 5);
        Assert.That(list.Count, Is.EqualTo(5));
    }

    [Test]
    public void EmitScorch_MarkedAsScorch_AndStaysLonger()
    {
        var list = new List<FloorSplatter>();
        var fx = NewFx();
        fx.EmitScorch(list, Vector2.Zero, radius: 50f, tint: Color.Orange, count: 4);
        Assert.That(list, Has.All.With.Property(nameof(FloorSplatter.IsScorch)).True);
        Assert.That(list, Has.All.With.Property(nameof(FloorSplatter.Lifetime)).GreaterThan(1.5f));
    }

    [Test]
    public void Tick_RemovesExpiredSplatters()
    {
        var list = new List<FloorSplatter>();
        var fx = NewFx();
        fx.EmitImpact(list, Vector2.Zero, Color.Red, count: 6);
        fx.Tick(list, dt: 2.0f);
        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Tick_DecaysVelocityOfMovingParticles()
    {
        var list = new List<FloorSplatter>
        {
            new()
            {
                Position = Vector2.Zero,
                Velocity = new Vector2(100, 0),
                Radius = 2f,
                Lifetime = 5f,
                MaxLifetime = 5f,
                Color = Color.White
            }
        };
        var fx = NewFx();
        fx.Tick(list, dt: 0.5f);
        Assert.That(list[0].Velocity.X, Is.LessThan(100f));
    }
}

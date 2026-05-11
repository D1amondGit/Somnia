using System;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class ZoneShapeFactoryTests
{
    [Test]
    public void BuildOrganicOutline_Deterministic_PerSeed()
    {
        var a = ZoneShapeFactory.BuildOrganicOutline(new Vector2(100, 100), 80f, new Random(7));
        var b = ZoneShapeFactory.BuildOrganicOutline(new Vector2(100, 100), 80f, new Random(7));

        Assert.That(a.Length, Is.EqualTo(b.Length));
        for (var i = 0; i < a.Length; i++)
            Assert.That(a[i], Is.EqualTo(b[i]));
    }

    [Test]
    public void BuildOrganicOutline_ProducesAtLeast13Points()
    {
        var pts = ZoneShapeFactory.BuildOrganicOutline(Vector2.Zero, 60f, new Random(1));
        Assert.That(pts.Length, Is.GreaterThanOrEqualTo(13));
    }

    [Test]
    public void BuildOrganicOutline_RadiusRespectedWithinReasonableBand()
    {
        var pts = ZoneShapeFactory.BuildOrganicOutline(Vector2.Zero, 100f, new Random(42));
        foreach (var p in pts)
        {
            var len = p.Length();
            Assert.That(len, Is.InRange(20f, 200f));
        }
    }
}

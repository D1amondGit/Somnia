using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class AnomalyZoneTests
{
    [Test]
    public void ContainsPoint_DefaultEllipse_UsesIsometricStretch()
    {
        var z = new AnomalyZone(Vector2.Zero, 100f, AnomalyType.Red);
        Assert.That(z.ContainsPoint(Vector2.Zero), Is.True);
        Assert.That(z.ContainsPoint(new Vector2(200, 0)), Is.False);
    }

    [Test]
    public void ContainsPoint_OrganicOutline_StillIncludesCenter()
    {
        var outline = ZoneShapeFactory.BuildOrganicOutline(new Vector2(500, 500), 120f, new System.Random(13));
        var z = new AnomalyZone(new Vector2(500, 500), 120f, AnomalyType.Green, outline);

        Assert.That(z.ContainsPoint(new Vector2(500, 500)), Is.True);
        Assert.That(z.ContainsPoint(new Vector2(500 + 400, 500)), Is.False);
    }

    [Test]
    public void Outline_Generated_HasAtLeastTriangleVertices()
    {
        var z = new AnomalyZone(new Vector2(10, 10), 50f, AnomalyType.Blue);
        Assert.That(z.Outline.Length, Is.GreaterThanOrEqualTo(3));
    }
}

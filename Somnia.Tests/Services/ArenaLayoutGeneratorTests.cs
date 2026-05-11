using System.Linq;
using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class ArenaLayoutGeneratorTests
{
    [Test]
    public void SameSeedProducesSameZonesCount_AsWallCountInvariant()
    {
        var gen = new ArenaLayoutGenerator();
        var play = new Rectangle(0, 0, 960, 640);
        var a = gen.Generate(play, seed: 5001).Zones.Count;
        var b = gen.Generate(play, seed: 5001).Zones.Count;

        Assert.That(b, Is.EqualTo(a));
    }

    [Test]
    public void Generate_IncludesInteriorObstacles()
    {
        var gen = new ArenaLayoutGenerator();
        var layout = gen.Generate(new Rectangle(0, 0, 1600, 900), seed: 7);
        Assert.That(layout.Walls.Count, Is.GreaterThan(15));
    }

    [Test]
    public void Generate_SpawnsMultipleColoredZones_WithOrganicOutlines()
    {
        var gen = new ArenaLayoutGenerator();
        var layout = gen.Generate(new Rectangle(0, 0, 1600, 900), seed: 99);

        Assert.That(layout.Zones.Count, Is.GreaterThanOrEqualTo(5));
        Assert.That(layout.Zones.All(z => z.Outline.Length >= 13), Is.True);
    }

    [Test]
    public void Generate_DistributesZonesAcrossArena_NotJustCenter()
    {
        var gen = new ArenaLayoutGenerator();
        var play = new Rectangle(0, 0, 1600, 900);
        var layout = gen.Generate(play, seed: 41);

        var xs = layout.Zones.Select(z => z.Center.X).ToArray();
        var ys = layout.Zones.Select(z => z.Center.Y).ToArray();

        Assert.That(xs.Min(), Is.LessThan(play.Width * 0.4f));
        Assert.That(xs.Max(), Is.GreaterThan(play.Width * 0.6f));
        Assert.That(ys.Min(), Is.LessThan(play.Height * 0.4f));
        Assert.That(ys.Max(), Is.GreaterThan(play.Height * 0.6f));
    }

    [Test]
    public void InteriorObstacles_AllUseFixedDimensions()
    {
        var gen = new ArenaLayoutGenerator();
        var layout = gen.Generate(new Rectangle(0, 0, 1600, 900), seed: 13);

        var interior = layout.Walls
            .Where(w => w.Radius <= ArenaLayoutGenerator.ObstacleRadius + 0.5f &&
                        w.Radius >= ArenaLayoutGenerator.ObstacleRadius - 0.5f)
            .ToArray();

        Assert.That(interior.Length, Is.GreaterThan(0));
        foreach (var w in interior)
        {
            Assert.That(w.WallHeight, Is.EqualTo(ArenaLayoutGenerator.ObstacleWallHeight));
            Assert.That(w.Squash, Is.EqualTo(ArenaHexGrid.Squash));
            Assert.That(w.Tilt, Is.EqualTo(ArenaHexGrid.Tilt));
        }
    }
}

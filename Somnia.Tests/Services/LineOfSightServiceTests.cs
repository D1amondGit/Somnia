using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class LineOfSightServiceTests
{
    [Test]
    public void BlockedWhenSegmentCrossesTopHexFace()
    {
        var los = new LineOfSightService();
        var blocker = new HexagonModel(new Vector2(200, 200), radius: 60f);
        var walls = new List<HexagonModel> { blocker };
        var top = blocker.GetTopVertices();
        var centroid = Vector2.Zero;
        foreach (var v in top)
            centroid += v;
        centroid /= top.Count;

        var from = centroid + new Vector2(-220f, 0f);
        var to = centroid + new Vector2(220f, 0f);
        Assert.That(los.HasLineOfSight(from, to, walls), Is.False);
    }

    [Test]
    public void ClearWhen_NoWalls()
    {
        var los = new LineOfSightService();
        Assert.That(los.HasLineOfSight(Vector2.Zero, new Vector2(500, 0), []), Is.True);
    }
}

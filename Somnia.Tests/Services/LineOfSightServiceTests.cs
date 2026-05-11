using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class LineOfSightServiceTests
{
    [Test]
    public void BlockedWhenSegmentCrossesFatHexCollider()
    {
        var los = new LineOfSightService();
        var blocker = new HexagonModel(new Vector2(200, 200), radius: 60f);
        var walls = new List<HexagonModel> { blocker };

        var clear = los.HasLineOfSight(new Vector2(100, 200), new Vector2(310, 200), walls);
        Assert.That(clear, Is.False);
    }

    [Test]
    public void ClearWhen_NoWalls()
    {
        var los = new LineOfSightService();
        Assert.That(los.HasLineOfSight(Vector2.Zero, new Vector2(500, 0), []), Is.True);
    }
}

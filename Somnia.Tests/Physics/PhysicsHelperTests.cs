using Somnia.Game.Models;

namespace Somnia.Tests.Physics;

[TestFixture]
public sealed class PhysicsHelperTests
{
    [Test]
    public void ResolveHexCollision_DisplacesPlayerInsideObstacleHull()
    {
        var hex = new HexagonModel(new Vector2(250, 250), radius: 50f);
        var pos = hex.Center - new Vector2(0, hex.WallHeight);
        var before = pos;

        PhysicsHelper.ResolveHexCollision(ref pos, 25f, hex);
        Assert.That(Vector2.DistanceSquared(before, pos), Is.GreaterThan(0.5f));
    }
}

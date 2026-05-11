using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class ZoneResolverTests
{
    [Test]
    public void LastOverlappingZone_WinsLikeLegacyCode()
    {
        var player = new PlayerModel(new Vector2(300, 300));
        var zones = new[]
        {
            new AnomalyZone(new Vector2(300, 300), 200f, AnomalyType.Red),
            new AnomalyZone(new Vector2(300, 300), 200f, AnomalyType.Blue)
        };

        ZoneResolver.RefreshPlayerZone(player, zones);
        Assert.That(player.CurrentZone, Is.EqualTo(AnomalyType.Blue));
    }
}

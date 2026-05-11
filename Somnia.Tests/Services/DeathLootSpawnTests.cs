using Somnia.Game.Models;
using Somnia.Game.Services.Economy;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class DeathLootSpawnTests
{
    [Test]
    public void CreatesTwoDrops_PerDeadEnemy()
    {
        var e = new EnemyModel(Vector2.Zero) { Health = 0f, HasDropped = false };
        var list = new List<EnemyModel> { e };
        var drops = new List<ResourceDropModel>();

        new DeathLootSpawnService().Process(list, drops);

        Assert.That(e.HasDropped, Is.True);
        Assert.That(drops.Count, Is.EqualTo(2));
    }
}

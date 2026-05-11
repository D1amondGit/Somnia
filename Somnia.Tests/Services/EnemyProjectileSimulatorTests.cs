using Somnia.Game.Models;
using Somnia.Game.Services.Projectiles;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class EnemyProjectileSimulatorTests
{
    [Test]
    public void DamagesPlayer_OnImpact()
    {
        var sim = new EnemyProjectileSimulator();
        var p = new PlayerModel(new Vector2(100, 100)) { CurrentHealth = 100f };
        var n = new NpcModel(new Vector2(500, 500)) { IsPickedUp = true };
        var pr = new List<ProjectileModel>
        {
            new(new Vector2(95, 100), Vector2.Zero, 8f) { LifeTime = 1f }
        };

        sim.Update(1f / 30f, pr, p, n);
        Assert.That(p.CurrentHealth, Is.LessThan(100f));
    }

    [Test]
    public void HitsNpc_OnGround_NotCarried()
    {
        var sim = new EnemyProjectileSimulator();
        var p = new PlayerModel(Vector2.One * -500f);
        var n = new NpcModel(new Vector2(50, 0)) { IsPickedUp = false, Health = 100f };

        var pr = new List<ProjectileModel>
        {
            new(new Vector2(40, 0), Vector2.Zero, 30f)
        };

        sim.Update(1f, pr, p, n);
        Assert.That(n.Health, Is.LessThan(100f));
        Assert.That(pr, Is.Empty);
    }
}

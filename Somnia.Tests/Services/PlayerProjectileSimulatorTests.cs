using Somnia.Game.Models;
using Somnia.Game.Services.Projectiles;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class PlayerProjectileSimulatorTests
{
    [Test]
    public void Bolt_TravelTime_BeforeDamage()
    {
        var los = new LineOfSightService();
        var sim = new PlayerProjectileSimulator(los);
        var enemy = new EnemyModel(new Vector2(400, 0)) { Health = 100f };
        var enemies = new List<EnemyModel> { enemy };
        var walls = new List<HexagonModel>();

        var bolt = new PlayerProjectileModel
        {
            Position = Vector2.Zero,
            Velocity = Vector2.UnitX * 1200f,
            Damage = 50f,
            Knockback = 0f,
            DamageSource = Vector2.Zero,
            LifeRemaining = 1f,
            Kind = PlayerProjectileKind.Bolt
        };
        var list = new List<PlayerProjectileModel> { bolt };

        sim.Update(1f / 1200f, list, enemies, walls);
        Assert.That(enemy.Health, Is.EqualTo(100f));

        for (var i = 0; i < 80 && enemy.Health > 99f; i++)
            sim.Update(1f / 120f, list, enemies, walls);

        Assert.That(enemy.Health, Is.LessThan(100f));
    }

    [Test]
    public void Rocket_ExplodesOnce_AndHitsCluster()
    {
        var sim = new PlayerProjectileSimulator(new LineOfSightService());
        var a = new EnemyModel(new Vector2(100, 0)) { Health = 200f };
        var b = new EnemyModel(new Vector2(130, 0)) { Health = 200f };
        var list = new List<PlayerProjectileModel>
        {
            new()
            {
                Position = Vector2.Zero,
                Velocity = Vector2.UnitX * 800f,
                Damage = 80f,
                Knockback = 0f,
                DamageSource = Vector2.Zero,
                LifeRemaining = 3f,
                Kind = PlayerProjectileKind.Rocket,
                ExplosionRadius = 150f
            }
        };

        for (var i = 0; i < 200 && list.Count > 0; i++)
            sim.Update(1f / 200f, list, new List<EnemyModel> { a, b }, new List<HexagonModel>());

        Assert.That(a.Health, Is.LessThan(200f));
        Assert.That(b.Health, Is.LessThan(200f));
    }
}

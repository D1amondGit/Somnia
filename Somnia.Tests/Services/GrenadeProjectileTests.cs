using Somnia.Game.Models;
using Somnia.Game.Services.Projectiles;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class GrenadeProjectileTests
{
    private static PlayerProjectileModel MakeGrenade(Vector2 pos, float explosionRadius = 200f) => new()
    {
        Position = pos,
        Velocity = Vector2.Zero,
        Damage = 30f,
        DamageSource = pos,
        LifeRemaining = 0.001f,
        ExplosionRadius = explosionRadius,
        HealAmount = 40f,
        PoisonDuration = 1.0f,
        Kind = PlayerProjectileKind.Grenade
    };

    [Test]
    public void Grenade_Detonates_HealsPlayer_AndNpcInRadius()
    {
        var sim = new PlayerProjectileSimulator(new LineOfSightService());
        var p = new PlayerModel(new Vector2(50, 0)) { CurrentHealth = 50f };
        var n = new NpcModel(new Vector2(70, 0)) { Health = 40f };
        var enemies = new List<EnemyModel>();
        var list = new List<PlayerProjectileModel> { MakeGrenade(new Vector2(60, 0)) };

        sim.Update(0.01f, list, enemies, [], p, n);

        Assert.That(p.CurrentHealth, Is.GreaterThan(50f), "Игрок должен получить хил");
        Assert.That(n.Health, Is.GreaterThan(40f), "NPC должен получить хил");
        Assert.That(list, Is.Empty, "Граната должна детонировать и удалиться");
    }

    [Test]
    public void Grenade_PoisonsEnemiesInRadius()
    {
        var sim = new PlayerProjectileSimulator(new LineOfSightService());
        var enemy = new EnemyModel(new Vector2(70, 0));
        var list = new List<PlayerProjectileModel> { MakeGrenade(new Vector2(60, 0)) };

        sim.Update(0.01f, list, [enemy], [],
            new PlayerModel(new Vector2(-500, -500)),
            new NpcModel(new Vector2(-500, -500)));

        Assert.That(enemy.IsInfected, Is.True);
        Assert.That(enemy.InfectionTimer, Is.GreaterThan(0f));
    }

    [Test]
    public void Grenade_DoesNotHealEntitiesOutsideRadius()
    {
        var sim = new PlayerProjectileSimulator(new LineOfSightService());
        var p = new PlayerModel(new Vector2(1000, 1000)) { CurrentHealth = 50f };
        var n = new NpcModel(new Vector2(1000, 1000)) { Health = 40f };
        var list = new List<PlayerProjectileModel> { MakeGrenade(new Vector2(0, 0), explosionRadius: 100f) };

        sim.Update(0.01f, list, [], [], p, n);

        Assert.That(p.CurrentHealth, Is.EqualTo(50f));
        Assert.That(n.Health, Is.EqualTo(40f));
    }
}

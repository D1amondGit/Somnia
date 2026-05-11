using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class ProjectileModelsTests
{
    [Test]
    public void PlayerProjectile_Defaults_AreSane()
    {
        var p = new PlayerProjectileModel
        {
            Position = Vector2.Zero,
            Velocity = Vector2.UnitX * 100f,
            Kind = PlayerProjectileKind.Bolt
        };
        Assert.That(p.LifeRemaining, Is.GreaterThan(0));
    }

    [Test]
    public void EnemyProjectile_Ctor_SetsFields()
    {
        var e = new ProjectileModel(Vector2.Zero, Vector2.UnitY * 10f, 5f);
        Assert.That(e.Radius, Is.EqualTo(5f));
    }
}

using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class EnemyModelTests
{
    [Test]
    public void TakeDamage_AppliesKnockback()
    {
        var e = new EnemyModel(Vector2.Zero) { Health = 100f };
        e.TakeDamage(10f, new Vector2(-10, 0), 400f);
        Assert.That(e.Health, Is.EqualTo(90f));
        Assert.That(e.Velocity.X, Is.GreaterThan(0));
    }

    [Test]
    public void Update_DampensVelocity()
    {
        var e = new EnemyModel(Vector2.Zero) { Velocity = new Vector2(1000, 0) };
        e.Update(1f / 60f);
        Assert.That(e.Velocity.Length(), Is.LessThan(1000));
    }

    [Test]
    public void IsDead_WhenHpZeroOrLess()
    {
        var e = new EnemyModel(Vector2.Zero) { Health = 0f };
        Assert.That(e.IsDead, Is.True);
    }
}

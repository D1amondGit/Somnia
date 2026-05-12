using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class PlayerKnockbackTests
{
    [Test]
    public void ApplyKnockback_AccumulatesVelocity()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.ApplyKnockback(new Vector2(100, 0));
        Assert.That(p.KnockbackVelocity.X, Is.EqualTo(100f));
    }

    [Test]
    public void TickCooldowns_AppliesKnockback_AndDecaysIt()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.ApplyKnockback(new Vector2(200, 0));
        var startKb = p.KnockbackVelocity.X;
        p.TickCooldowns(0.1f);

        Assert.That(p.Position.X, Is.GreaterThan(0f),
            "Knockback должен сместить позицию");
        Assert.That(p.KnockbackVelocity.X, Is.LessThan(startKb),
            "Knockback velocity должна затухать");
    }

    [Test]
    public void ResetForRun_ZeroesKnockback()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.ApplyKnockback(new Vector2(500, 200));
        p.ResetForRun();
        Assert.That(p.KnockbackVelocity, Is.EqualTo(Vector2.Zero));
    }
}

using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class NpcModelTests
{
    [Test]
    public void IsInjured_WhenBelowHalf_MaxHealth_Default100()
    {
        var n = new NpcModel(Vector2.Zero) { Health = 49f, MaxHealth = 100f };
        Assert.That(n.IsInjured, Is.True);
    }

    [Test]
    public void TakeDamage_ClampedToZero()
    {
        var n = new NpcModel(Vector2.Zero) { Health = 5f };
        n.TakeDamage(999f);
        Assert.That(n.Health, Is.Zero);
        Assert.That(n.IsDead, Is.True);
    }

    [Test]
    public void ResetForRun_FillsHp_AndClearsPickup()
    {
        var n = new NpcModel(Vector2.Zero) { Health = 2f, IsPickedUp = true };
        n.ResetForRun();
        Assert.That(n.Health, Is.EqualTo(n.MaxHealth));
        Assert.That(n.IsPickedUp, Is.False);
    }
}

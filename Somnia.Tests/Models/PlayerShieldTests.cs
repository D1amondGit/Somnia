using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class PlayerShieldTests
{
    [Test]
    public void Shield_ReducesIncomingDamageByConfiguredFraction()
    {
        var p = new PlayerModel(Vector2.Zero);
        var hpBefore = p.CurrentHealth;
        p.BeginShield(durationSeconds: 5f, radius: 200f, reduction: 0.5f);

        p.TakeDamage(40f);

        Assert.That(hpBefore - p.CurrentHealth, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void Shield_NoEffectAfterExpiry()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.BeginShield(durationSeconds: 0.2f, radius: 200f, reduction: 0.8f);
        p.TickCooldowns(0.5f);

        Assert.That(p.IsShieldActive, Is.False);
        var hp = p.CurrentHealth;
        p.TakeDamage(20f);
        Assert.That(hp - p.CurrentHealth, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void Shield_PushesEnemiesInRangeOutward()
    {
        var p = new PlayerModel(new Vector2(100, 100));
        var enemy = new EnemyModel(new Vector2(180, 100));
        p.BeginShield(durationSeconds: 1f, radius: 200f);

        p.TickGreenAura(0.1f, new[] { enemy });

        Assert.That(enemy.Position.X, Is.GreaterThan(180f));
    }

    [Test]
    public void ResetForRun_ClearsShieldState()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.BeginShield(2f);
        p.ResetForRun();
        Assert.That(p.IsShieldActive, Is.False);
        Assert.That(p.ShieldTimer, Is.EqualTo(0f));
    }
}

using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class EnemyArchetypeCatalogTests
{
    [Test]
    public void EveryEnemyType_HasArchetype_WithPositiveHealthAndSpeed()
    {
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            var a = EnemyArchetypeCatalog.Get(type);

            Assert.That(a, Is.Not.Null, $"{type} archetype is null");
            Assert.That(a.Type, Is.EqualTo(type));
            Assert.That(a.MaxHealth, Is.GreaterThan(0f));
            Assert.That(a.MoveSpeed, Is.GreaterThan(0f));
            Assert.That(a.BodyRadius, Is.GreaterThan(0f));
            Assert.That(a.BodyHeight, Is.GreaterThan(0f));
        }
    }

    [Test]
    public void Charger_HasLowerHealth_AndHigherSpeed_ThanMelee()
    {
        var charger = EnemyArchetypeCatalog.Get(EnemyType.Charger);
        var melee = EnemyArchetypeCatalog.Get(EnemyType.Melee);

        Assert.That(charger.MoveSpeed, Is.GreaterThan(melee.MoveSpeed));
        Assert.That(charger.MaxHealth, Is.LessThan(melee.MaxHealth));
        Assert.That(charger.ExplodesOnContact, Is.True);
    }

    [Test]
    public void Sniper_HasTelegraph_AndLongPreferredRange()
    {
        var sniper = EnemyArchetypeCatalog.Get(EnemyType.Sniper);
        var shooter = EnemyArchetypeCatalog.Get(EnemyType.Shooter);

        Assert.That(sniper.TelegraphTime, Is.GreaterThan(0f));
        Assert.That(sniper.PreferredRange, Is.GreaterThan(shooter.PreferredRange));
        Assert.That(sniper.ProjectileSpeed, Is.GreaterThan(shooter.ProjectileSpeed));
    }

    [Test]
    public void EnemyModel_TakesMaxHealth_FromArchetype()
    {
        var sniper = new EnemyModel(Vector2.Zero, EnemyType.Sniper);
        Assert.That(sniper.MaxHealth, Is.EqualTo(EnemyArchetypeCatalog.Get(EnemyType.Sniper).MaxHealth));
        Assert.That(sniper.Health, Is.EqualTo(sniper.MaxHealth));
    }
}

using Somnia.Game.Models;
using Somnia.Game.Services.Combat;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class PlayerCombatServiceTests
{
    [Test]
    public void NeutralAutomatic_FiresBurst_OfThreeProjectiles()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentMana = 100f,
            CurrentZone = AnomalyType.Neutral,
            ActiveSlot = 0,
            State = PlayerState.Free
        };

        var projs = new List<PlayerProjectileModel>();
        var ok = svc.TryUseActiveSkill(p, new Vector2(300, 0), [], new NpcModel(new Vector2(900, 900)), [], projs);

        Assert.That(ok, Is.True);
        Assert.That(projs.Count, Is.EqualTo(3));
        Assert.That(projs[0].Velocity.Length(), Is.GreaterThan(800f));
        Assert.That(projs.All(pr => pr.Kind == PlayerProjectileKind.Bolt), Is.True);
    }

    [Test]
    public void GreenGrenade_Spawns_WithoutRequiringTarget()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentMana = 80f,
            CurrentZone = AnomalyType.Green,
            ActiveSlot = 0
        };
        var projs = new List<PlayerProjectileModel>();

        Assert.That(svc.TryUseActiveSkill(p, Vector2.UnitX, [], new NpcModel(new Vector2(500, 500)), [], projs),
            Is.True);
        Assert.That(projs, Has.One.Items);
        Assert.That(projs[0].Kind, Is.EqualTo(PlayerProjectileKind.Grenade));
        Assert.That(projs[0].HealAmount, Is.GreaterThan(0f));
        Assert.That(projs[0].PoisonDuration, Is.GreaterThan(0f));
    }

    [Test]
    public void Shotgun_SpawnsMultiplePellets_AndApplyKnockback()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentMana = 120f,
            CurrentZone = AnomalyType.Red,
            ActiveSlot = 0
        };

        var projs = new List<PlayerProjectileModel>();
        Assert.That(svc.TryUseActiveSkill(p, new Vector2(1, 0.1f), [], new NpcModel(new Vector2(600, 0)), [], projs),
            Is.True);
        Assert.That(projs.Count, Is.EqualTo(7));
        Assert.That(p.KnockbackVelocity.Length(), Is.GreaterThan(0f),
            "Дробовик должен толкнуть игрока назад (отдача)");
    }

    [Test]
    public void BlueSniper_HasHugeRange_AndHugeDamage()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentMana = 100f,
            CurrentZone = AnomalyType.Blue,
            ActiveSlot = 0
        };
        var projs = new List<PlayerProjectileModel>();

        Assert.That(svc.TryUseActiveSkill(p, new Vector2(200, 0), [], new NpcModel(new Vector2(900, 900)), [], projs),
            Is.True);
        Assert.That(projs, Has.One.Items);
        Assert.That(projs[0].MaxTravelDistance, Is.GreaterThan(1500f));
        Assert.That(projs[0].Damage, Is.GreaterThan(100f));
        Assert.That(projs[0].Velocity.Length(), Is.GreaterThan(1500f));
    }

    [Test]
    public void GreenShield_ActivatesShield_AndPushesEnemiesInRange()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(new Vector2(500, 500))
        {
            CurrentMana = 100f,
            CurrentZone = AnomalyType.Green,
            ActiveSlot = 1,
        };
        var enemy = new EnemyModel(new Vector2(560, 500));
        var enemies = new List<EnemyModel> { enemy };
        var projs = new List<PlayerProjectileModel>();
        var v0 = enemy.Velocity;

        var ok = svc.TryUseActiveSkill(p, new Vector2(1, 0), enemies, new NpcModel(new Vector2(0, 0)), [], projs);

        Assert.That(ok, Is.True);
        Assert.That(p.IsShieldActive, Is.True);
        Assert.That(enemy.Velocity.Length(), Is.GreaterThan(v0.Length()),
            "Активный щит должен резко отталкивать врагов в радиусе");
    }

    [Test]
    public void GreenInfect_DamagesAndInfectsClosestTarget()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(new Vector2(100, 100))
        {
            CurrentMana = 100f,
            CurrentZone = AnomalyType.Green,
            ActiveSlot = 2,
        };
        var enemy = new EnemyModel(new Vector2(140, 100));
        var enemies = new List<EnemyModel> { enemy };
        var hpBefore = enemy.Health;

        var ok = svc.TryUseActiveSkill(p, new Vector2(1, 0), enemies, new NpcModel(new Vector2(0, 0)), [],
            new List<PlayerProjectileModel>());

        Assert.That(ok, Is.True);
        Assert.That(enemy.IsInfected, Is.True);
        Assert.That(enemy.Health, Is.LessThan(hpBefore));
    }
}

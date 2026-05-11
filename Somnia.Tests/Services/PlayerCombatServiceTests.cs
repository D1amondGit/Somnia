using Somnia.Game.Models;
using Somnia.Game.Services.Combat;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class PlayerCombatServiceTests
{
    [Test]
    public void NeutralBolt_SpawnsProjectile_WhenRangeValid()
    {
        var los = new LineOfSightService();
        var svc = new PlayerCombatService(los);
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
        Assert.That(projs, Has.One.Items);
        Assert.That(projs[0].Velocity.Length(), Is.GreaterThan(500f));
    }

    [Test]
    public void GreenBolt_DoesNotConsumeMana_WhenNoTarget()
    {
        var svc = new PlayerCombatService(new LineOfSightService());
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentMana = 80f,
            CurrentZone = AnomalyType.Green,
            ActiveSlot = 0,
            MaxCd1 = 0.8f
        };
        var projs = new List<PlayerProjectileModel>();
        Assert.That(svc.TryUseActiveSkill(p, Vector2.UnitX, [], new NpcModel(new Vector2(500, 500)), [], projs),
            Is.False);
        Assert.That(p.CurrentMana, Is.EqualTo(80f));
        Assert.That(projs, Is.Empty);
    }

    [Test]
    public void Shotgun_SpawnsMultiplePellets()
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
        Assert.That(projs.Count, Is.EqualTo(6));
    }
}

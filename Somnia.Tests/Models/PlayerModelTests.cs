using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class PlayerModelTests
{
    [Test]
    public void CarryState_DoesNotChangeSpeedConstants()
    {
        Assert.That(PlayerModel.SpeedCarrying, Is.EqualTo(150f));
        Assert.That(PlayerModel.SpeedFree, Is.EqualTo(300f));
    }

    [Test]
    public void RegisterSkill_AppliesCooldownToActiveSlot()
    {
        var p = new PlayerModel(Vector2.Zero) { ActiveSlot = 1, MaxCd2 = 2f };
        p.RegisterSkillExecuted();
        Assert.That(p.Cd2, Is.EqualTo(2f).Within(0.001f));
    }

    [Test]
    public void ConsumeMana_Fails_WhenPoolTooLow()
    {
        var p = new PlayerModel(Vector2.Zero) { CurrentMana = 5f };
        Assert.That(p.ConsumeMana(10f), Is.False);
        Assert.That(p.CurrentMana, Is.EqualTo(5f));
    }

    [Test]
    public void SkillForcedDash_SetsDashingWindow()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.ActivateSkillForcedDash();
        Assert.That(p.IsDashing, Is.True);
    }

    [Test]
    public void TickGreenAura_DamagesEnemyInRadius()
    {
        var p = new PlayerModel(Vector2.Zero);
        p.BeginGreenAura(0.2f);
        var e = new EnemyModel(new Vector2(50, 0)) { Health = 100f };
        p.TickGreenAura(0.1f, new[] { e });
        Assert.That(e.Health, Is.LessThan(100f));
    }

    [Test]
    public void ResetForRun_RestoresHealthManaAndClearsAuras()
    {
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentHealth = 0f,
            CurrentMana = 0f,
            Cd1 = 5f,
            GreenAuraTimer = 3f,
            DamageMultiplier = 0.25f,
            ActiveSlot = 2
        };
        p.ResetForRun();
        Assert.That(p.CurrentHealth, Is.EqualTo(p.MaxHealth));
        Assert.That(p.CurrentMana, Is.EqualTo(p.MaxMana));
        Assert.That(p.IsDead, Is.False);
        Assert.That(p.Cd1, Is.Zero);
        Assert.That(p.GreenAuraTimer, Is.Zero);
        Assert.That(p.ActiveSlot, Is.Zero);
        Assert.That(p.DamageMultiplier, Is.EqualTo(1f));
    }
}

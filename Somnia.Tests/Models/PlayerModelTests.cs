using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class PlayerModelTests
{
    [Test]
    public void CarrySpeed_IsCloseEnoughToFreeSpeed_ForPaceCheck()
    {
        // После рефакторинга темпа: с заложником бежим лишь немного медленнее.
        Assert.That(PlayerModel.SpeedCarrying, Is.GreaterThan(250f),
            "Перенос NPC не должен превращать игрока в улитку");
        Assert.That(PlayerModel.SpeedFree, Is.GreaterThan(PlayerModel.SpeedCarrying));
        Assert.That(PlayerModel.SpeedDashing, Is.GreaterThan(PlayerModel.SpeedFree));
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
    public void TickGreenAura_PushesEnemyInRadius()
    {
        // Зелёная аура была заменена на «Щит»: уже не наносит урон, только отталкивает.
        var p = new PlayerModel(Vector2.Zero);
        p.BeginGreenAura(0.2f);
        var e = new EnemyModel(new Vector2(50, 0)) { Health = 100f };
        p.TickGreenAura(0.1f, new[] { e });

        Assert.That(p.IsShieldActive, Is.True);
        Assert.That(e.Position.X, Is.GreaterThan(50f),
            "После тика щит должен отталкивать врагов наружу");
    }

    [Test]
    public void ResetForRun_RestoresHealthManaAndClearsAuras()
    {
        var p = new PlayerModel(Vector2.Zero)
        {
            CurrentHealth = 0f,
            CurrentMana = 0f,
            Cd1 = 5f,
            DamageMultiplier = 0.25f,
            ActiveSlot = 2
        };
        p.BeginGreenAura(3f);
        p.ResetForRun();
        Assert.That(p.CurrentHealth, Is.EqualTo(p.MaxHealth));
        Assert.That(p.CurrentMana, Is.EqualTo(p.MaxMana));
        Assert.That(p.IsDead, Is.False);
        Assert.That(p.Cd1, Is.Zero);
        Assert.That(p.GreenAuraTimer, Is.Zero);
        Assert.That(p.IsShieldActive, Is.False);
        Assert.That(p.ActiveSlot, Is.Zero);
        Assert.That(p.DamageMultiplier, Is.EqualTo(1f));
    }
}

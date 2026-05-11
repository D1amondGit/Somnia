using Somnia.Game.Models;
using Somnia.Game.Services.Economy;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class ResourceDropOrchestratorTests
{
    [Test]
    public void ManaPickup_IncreasesPlayerMana()
    {
        var pl = new PlayerModel(Vector2.Zero) { CurrentMana = 10f, MaxMana = 100f };
        var drops = new List<ResourceDropModel>
        {
            new(Vector2.One, DropType.Mana, 35f)
        };
        drops[0].Update(pl.Position, 1f); // collect

        var texts = new List<FloatingText>();
        new ResourceDropOrchestrator().Update(1f / 60f, pl, drops, texts);

        Assert.That(pl.CurrentMana, Is.GreaterThan(10f));
        Assert.That(texts.Exists(t => t.Text.Contains("MP")), Is.True);
    }
}

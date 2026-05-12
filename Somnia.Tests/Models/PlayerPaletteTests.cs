using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class PlayerPaletteTests
{
    [Test]
    public void GetBodyColor_DiffersByZone()
    {
        var p = new PlayerModel(Vector2.Zero) { ActiveSlot = 0 };

        p.CurrentZone = AnomalyType.Red;
        var red = PlayerPalette.GetBodyColor(p);

        p.CurrentZone = AnomalyType.Blue;
        var blue = PlayerPalette.GetBodyColor(p);

        p.CurrentZone = AnomalyType.Green;
        var green = PlayerPalette.GetBodyColor(p);

        Assert.That(red, Is.Not.EqualTo(blue));
        Assert.That(blue, Is.Not.EqualTo(green));
        Assert.That(green, Is.Not.EqualTo(red));
    }

    [Test]
    public void GetBodyColor_DiffersBySlot_WithinSameZone()
    {
        var p = new PlayerModel(Vector2.Zero) { CurrentZone = AnomalyType.Red };

        p.ActiveSlot = 0;
        var basic = PlayerPalette.GetBodyColor(p);
        p.ActiveSlot = 2;
        var ult = PlayerPalette.GetBodyColor(p);

        Assert.That(basic, Is.Not.EqualTo(ult));
    }

    [Test]
    public void GetZoneTint_MatchesEachKnownAnomaly()
    {
        Assert.That(PlayerPalette.GetZoneTint(AnomalyType.Red), Is.Not.EqualTo(PlayerPalette.GetZoneTint(AnomalyType.Blue)));
        Assert.That(PlayerPalette.GetZoneTint(AnomalyType.Neutral), Is.Not.EqualTo(PlayerPalette.GetZoneTint(AnomalyType.Green)));
    }
}

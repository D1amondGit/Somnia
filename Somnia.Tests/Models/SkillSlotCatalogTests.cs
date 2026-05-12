using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class SkillSlotCatalogTests
{
    [TestCase(AnomalyType.Neutral, 0, SkillIconShape.Rifle)]
    [TestCase(AnomalyType.Red, 0, SkillIconShape.Shotgun)]
    [TestCase(AnomalyType.Blue, 0, SkillIconShape.Sniper)]
    [TestCase(AnomalyType.Green, 0, SkillIconShape.Grenade)]
    [TestCase(AnomalyType.Green, 1, SkillIconShape.Aura)]
    [TestCase(AnomalyType.Green, 2, SkillIconShape.Infect)]
    public void Get_ReturnsExpectedIcon(AnomalyType zone, int slot, SkillIconShape shape)
    {
        var icon = SkillSlotCatalog.Get(zone, slot);
        Assert.That(icon.Icon, Is.EqualTo(shape));
        Assert.That(icon.Title, Is.Not.Null.And.Not.Empty);
    }
}

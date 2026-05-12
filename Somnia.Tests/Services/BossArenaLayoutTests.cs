using System.Linq;
using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class BossArenaLayoutTests
{
    [Test]
    public void BossArena_HasDestructibleCovers_AndShiftingAnomalyZones()
    {
        var layout = BossArenaLayout.Build(new Rectangle(0, 0, 1280, 720), seed: 42);

        var destructible = layout.Walls.Count(w => w.IsDestructible);

        Assert.That(destructible, Is.GreaterThan(0));
        Assert.That(layout.Zones.Count, Is.GreaterThanOrEqualTo(4),
            "На босс-арене должны быть цветные зоны, типы которых крутит оркестратор.");
    }

    [Test]
    public void BossArena_KeepsDeterministic_BySeed()
    {
        var a = BossArenaLayout.Build(new Rectangle(0, 0, 1280, 720), seed: 7);
        var b = BossArenaLayout.Build(new Rectangle(0, 0, 1280, 720), seed: 7);

        Assert.That(a.Walls.Count, Is.EqualTo(b.Walls.Count));
    }
}

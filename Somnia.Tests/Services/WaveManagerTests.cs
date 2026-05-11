using Somnia.Game.Services.Waves;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class WaveManagerTests
{
    [Test]
    public void Spawn_CurrentArenaIncreasesEnemyCount_LikeDesign()
    {
        var wm = new WaveManager();
        var a0 = wm.SpawnCurrentWave(800, 600).Count;

        wm.AdvanceArena();
        var a1 = wm.SpawnCurrentWave(800, 600).Count;

        wm.AdvanceArena();
        var a2 = wm.SpawnCurrentWave(800, 600).Count;

        Assert.That(a1, Is.GreaterThan(a0));
        Assert.That(a2, Is.GreaterThan(a1));
    }

    [Test]
    public void AllArenasCleared_AfterEnoughAdvances()
    {
        var wm = new WaveManager();
        for (var i = 0; i < WaveManager.ArenaCount; i++) wm.AdvanceArena();
        Assert.That(wm.AllArenasCleared, Is.True);
    }
}

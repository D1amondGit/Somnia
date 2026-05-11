using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class GateModelTests
{
    [Test]
    public void TryOpen_OpensOnlyWhenNpcCarrying_And_CloseToGate_AndAllEnemiesKilled()
    {
        var g = new GateModel(new Vector2(500, 400));
        var p = new PlayerModel(new Vector2(500, 400)) { State = PlayerState.Free };
        var n = new NpcModel(new Vector2(500, 400)) { IsPickedUp = false };

        g.TryOpen(p, n, aliveEnemies: 0, totalEnemies: 0);
        Assert.That(g.IsOpen, Is.False);

        n.IsPickedUp = true;
        p.SetState(PlayerState.Carrying);
        g.TryOpen(p, n, aliveEnemies: 0, totalEnemies: 0);
        Assert.That(g.IsOpen, Is.True);
    }

    [Test]
    public void TryOpen_StaysClosed_IfNpcDead()
    {
        var g = new GateModel(Vector2.One * 400);
        var p = new PlayerModel(Vector2.One * 400) { State = PlayerState.Carrying };
        var n = new NpcModel(Vector2.One * 400)
        {
            IsPickedUp = true,
            Health = 0f
        };
        g.TryOpen(p, n, aliveEnemies: 0, totalEnemies: 0);
        Assert.That(g.IsOpen, Is.False);
    }

    [Test]
    public void TryOpen_StaysClosed_IfNotEnoughEnemiesKilled()
    {
        var g = new GateModel(new Vector2(500, 400)) { MinKillFraction = 0.5f };
        var p = new PlayerModel(new Vector2(500, 400));
        p.SetState(PlayerState.Carrying);
        var n = new NpcModel(new Vector2(500, 400)) { IsPickedUp = true };

        // Убито 2/10 = 20% — гейт закрыт.
        g.TryOpen(p, n, aliveEnemies: 8, totalEnemies: 10);
        Assert.That(g.IsOpen, Is.False);

        // Убито 6/10 = 60% — гейт открывается.
        g.TryOpen(p, n, aliveEnemies: 4, totalEnemies: 10);
        Assert.That(g.IsOpen, Is.True);
    }

    [Test]
    public void TryOpen_IgnoresKillRequirement_WhenFlagSet()
    {
        var g = new GateModel(new Vector2(500, 400))
        {
            MinKillFraction = 0.99f,
            IgnoreKillRequirement = true
        };
        var p = new PlayerModel(new Vector2(500, 400));
        p.SetState(PlayerState.Carrying);
        var n = new NpcModel(new Vector2(500, 400)) { IsPickedUp = true };

        g.TryOpen(p, n, aliveEnemies: 10, totalEnemies: 10);
        Assert.That(g.IsOpen, Is.True);
    }
}

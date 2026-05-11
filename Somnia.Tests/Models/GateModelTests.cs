using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class GateModelTests
{
    [Test]
    public void TryOpen_OpensOnlyWhenNpcCarrying_And_CloseToGate()
    {
        var g = new GateModel(new Vector2(500, 400));
        var p = new PlayerModel(new Vector2(500, 400)) { State = PlayerState.Free };
        var n = new NpcModel(new Vector2(500, 400)) { IsPickedUp = false };

        g.TryOpen(p, n);
        Assert.That(g.IsOpen, Is.False);

        n.IsPickedUp = true;
        p.SetState(PlayerState.Carrying);
        g.TryOpen(p, n);
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
        g.TryOpen(p, n);
        Assert.That(g.IsOpen, Is.False);
    }
}

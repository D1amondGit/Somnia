using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.Npc;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class NpcCarryInteractionTests
{
    [Test]
    public void Carry_Starts_OnKeyDownEdge()
    {
        var svc = new NpcCarryInteractionService();
        var p = new PlayerModel(new Vector2(100, 100));
        var n = new NpcModel(new Vector2(120, 100));

        svc.TryToggle(new KeyboardState(), new KeyboardState(Keys.E), p, n);

        Assert.That(p.State, Is.EqualTo(PlayerState.Carrying));
        Assert.That(n.IsPickedUp, Is.True);
    }

    [Test]
    public void Carry_NoTrigger_WhenHoldAcrossFramesWithoutEdgeSemantics()
    {
        var svc = new NpcCarryInteractionService();
        var p = new PlayerModel(new Vector2(100, 100));
        var n = new NpcModel(new Vector2(120, 100));
        var held = new KeyboardState(Keys.E);
        svc.TryToggle(held, held, p, n);
        Assert.That(p.State, Is.EqualTo(PlayerState.Free));
    }

    [Test]
    public void DropCarriedNpc_ClearsCarry_AndNoOpWhenFree()
    {
        var p = new PlayerModel(new Vector2(100, 100)) { State = PlayerState.Carrying };
        var n = new NpcModel(new Vector2(120, 100)) { IsPickedUp = true };

        NpcCarryInteractionService.DropCarriedNpc(p, n);
        Assert.That(p.State, Is.EqualTo(PlayerState.Free));
        Assert.That(n.IsPickedUp, Is.False);

        NpcCarryInteractionService.DropCarriedNpc(p, n);
        Assert.That(p.State, Is.EqualTo(PlayerState.Free));
    }
}

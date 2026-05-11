using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Session;

namespace Somnia.Tests.Controllers;

[TestFixture]
public sealed class GameplayOrchestratorTests
{
    private sealed class ConstSeedRng : Random
    {
        public override int Next() => 42;
    }

    [Test]
    public void RestartGame_SpawnsEnemies_AndLayoutsArena()
    {
        var player = new PlayerModel(Vector2.Zero);
        var session = new GameplaySessionState
        {
            Player = player,
            Npc = new NpcModel(Vector2.One)
        };

        var orch = new GameplayOrchestrator(player);
        orch.RestartGame(session, 1200, 800, new ConstSeedRng());

        Assert.That(session.Zones.Count, Is.GreaterThan(0));
        Assert.That(session.Walls.Count, Is.GreaterThan(0));
        Assert.That(session.Enemies.Count, Is.GreaterThan(0));
        Assert.That(session.Gates, Has.One.Items);
        Assert.That(session.UiState, Is.Zero);
    }

    [Test]
    public void TryAdvanceArena_AfterThirdTransition_SetsGameOver()
    {
        var player = new PlayerModel(Vector2.Zero);
        var session = new GameplaySessionState
        {
            Player = player,
            Npc = new NpcModel(Vector2.One)
        };

        var orch = new GameplayOrchestrator(player);
        orch.RestartGame(session, 960, 640, new ConstSeedRng());

        Assert.That(orch.TryAdvanceArena(session, new Random(1)), Is.True);
        Assert.That(session.UiState, Is.Zero);
        Assert.That(orch.TryAdvanceArena(session, new Random(2)), Is.True);
        Assert.That(session.UiState, Is.Zero);
        Assert.That(orch.TryAdvanceArena(session, new Random(3)), Is.False);
        Assert.That(session.UiState, Is.EqualTo(2));
    }

    [Test]
    public void SimulatePlayingFrame_Smoke_MovementDoesNotCrash()
    {
        var player = new PlayerModel(new Vector2(400, 400));
        var session = new GameplaySessionState
        {
            Player = player,
            Npc = new NpcModel(new Vector2(405, 400))
        };
        var orch = new GameplayOrchestrator(player);
        orch.RestartGame(session, 800, 600, new Random(9));

        Assert.DoesNotThrow(() =>
            orch.SimulatePlayingFrame(session, 1f / 60f, default, default, Matrix.Identity));
    }
}

using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Session;

namespace Somnia.Tests.Controllers;

[TestFixture]
public sealed class WipeoutAdvanceTests
{
    [Test]
    public void NoLivingEnemies_AfterDelay_AdvancesArenaAutomatically()
    {
        var player = new PlayerModel(new Vector2(400, 400));
        var session = new GameplaySessionState
        {
            Player = player,
            Npc = new NpcModel(new Vector2(420, 400))
        };

        var orch = new GameplayOrchestrator(player);
        orch.RestartGame(session, 1200, 800, new Random(7));

        foreach (var e in session.Enemies) e.Health = 0f;
        var startArena = session.Waves.CurrentArena;

        for (var i = 0; i < 200; i++)
        {
            orch.SimulatePlayingFrame(session, 1f / 30f, default, default, Matrix.Identity);
            if (session.Waves.CurrentArena > startArena) break;
        }

        Assert.That(session.Waves.CurrentArena, Is.GreaterThan(startArena),
            "После убийства всех врагов оркестратор должен автоматически перейти на следующую арену");
    }

    [Test]
    public void NpcAliveAndWell_GrantsDamageBonus()
    {
        var player = new PlayerModel(Vector2.Zero);
        var session = new GameplaySessionState
        {
            Player = player,
            Npc = new NpcModel(Vector2.Zero)
        };

        var orch = new GameplayOrchestrator(player);
        orch.RestartGame(session, 1200, 800, new Random(11));

        orch.SimulatePlayingFrame(session, 1f / 60f, default, default, Matrix.Identity);
        Assert.That(player.DamageMultiplier, Is.GreaterThan(1f), "Живой NPC даёт бонус к урону");

        session.Npc.Health = 1f;
        orch.SimulatePlayingFrame(session, 1f / 60f, default, default, Matrix.Identity);
        Assert.That(player.DamageMultiplier, Is.LessThan(1f), "Раненый NPC снижает множитель");

        session.Npc.Health = 0f;
        orch.SimulatePlayingFrame(session, 1f / 60f, default, default, Matrix.Identity);
        Assert.That(player.DamageMultiplier, Is.LessThan(0.7f), "Мёртвый NPC — серьёзный штраф");
    }
}

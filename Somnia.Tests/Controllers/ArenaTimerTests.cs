using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Services.Waves;
using Somnia.Game.Session;

namespace Somnia.Tests.Controllers;

[TestFixture]
public sealed class ArenaTimerTests
{
    private static GameplaySessionState NewSession()
    {
        var s = new GameplaySessionState
        {
            Player = new PlayerModel(new Vector2(100, 100)),
            Npc = new NpcModel(new Vector2(200, 200)),
            Waves = new WaveManager(),
            PlayArea = new Rectangle(0, 0, 1600, 900),
            ArenaTimer = GameplayOrchestrator.ArenaTimerMaxSeconds,
        };
        return s;
    }

    [Test]
    public void Restart_SetsArenaTimerToMax()
    {
        var s = NewSession();
        s.ArenaTimer = 5f;
        var orch = new GameplayOrchestrator(s.Player);
        orch.RestartGame(s, 1600, 900, new System.Random(1));
        Assert.That(s.ArenaTimer, Is.EqualTo(GameplayOrchestrator.ArenaTimerMaxSeconds));
    }

    [Test]
    public void ArenaTimer_TicksDownDuringPlay()
    {
        var s = NewSession();
        var orch = new GameplayOrchestrator(s.Player);
        orch.RestartGame(s, 1600, 900, new System.Random(2));

        var ks = new KeyboardState();
        orch.SimulatePlayingFrame(s, dt: 1.0f, ks, ks, Matrix.Identity);

        Assert.That(s.ArenaTimer, Is.LessThan(GameplayOrchestrator.ArenaTimerMaxSeconds));
    }

    [Test]
    public void Overtime_DealsDamageToPlayer()
    {
        var s = NewSession();
        var orch = new GameplayOrchestrator(s.Player);
        orch.RestartGame(s, 1600, 900, new System.Random(3));
        s.ArenaTimer = 0f;
        s.OvertimeElapsed = 0f;

        var hpBefore = s.Player.CurrentHealth;
        var ks = new KeyboardState();
        for (var i = 0; i < 60; i++)
            orch.SimulatePlayingFrame(s, dt: 1f / 60f, ks, ks, Matrix.Identity);

        Assert.That(s.Player.CurrentHealth, Is.LessThan(hpBefore));
    }
}

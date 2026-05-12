using Microsoft.Xna.Framework.Input;
using Somnia.Game.Controllers;
using Somnia.Game.Models;

namespace Somnia.Tests.Controllers;

[TestFixture]
public sealed class MenuControllerTests
{
    [Test]
    public void Title_PressEnter_RequestsStart()
    {
        var c = new MenuController();
        var cmd = c.Update(EmptyState, WithKey(Keys.Enter), GameplayPhase.Title);
        Assert.That(cmd, Is.EqualTo(MenuCommand.StartNewRun));
    }

    [Test]
    public void Title_PressQ_RequestsQuit()
    {
        var c = new MenuController();
        var cmd = c.Update(EmptyState, WithKey(Keys.Q), GameplayPhase.Title);
        Assert.That(cmd, Is.EqualTo(MenuCommand.Quit));
    }

    [Test]
    public void Paused_PressEscape_RequestsResume()
    {
        var c = new MenuController();
        var cmd = c.Update(EmptyState, WithKey(Keys.Escape), GameplayPhase.Paused);
        Assert.That(cmd, Is.EqualTo(MenuCommand.Resume));
    }

    [Test]
    public void GameOver_PressEnter_RequestsRestart()
    {
        var c = new MenuController();
        var cmd = c.Update(EmptyState, WithKey(Keys.Enter), GameplayPhase.GameOver);
        Assert.That(cmd, Is.EqualTo(MenuCommand.RestartRun));
    }

    [Test]
    public void GameOver_PressEscape_RequestsTitle()
    {
        var c = new MenuController();
        var cmd = c.Update(EmptyState, WithKey(Keys.Escape), GameplayPhase.GameOver);
        Assert.That(cmd, Is.EqualTo(MenuCommand.ReturnToTitle));
    }

    [Test]
    public void HoldingKey_DoesNotRetrigger_OnFollowupFrame()
    {
        var c = new MenuController();
        var state = WithKey(Keys.Enter);
        Assert.That(c.Update(EmptyState, state, GameplayPhase.Title), Is.EqualTo(MenuCommand.StartNewRun));
        Assert.That(c.Update(state, state, GameplayPhase.Title), Is.EqualTo(MenuCommand.None));
    }

    private static KeyboardState EmptyState => new();

    private static KeyboardState WithKey(Keys k) => new(k);
}

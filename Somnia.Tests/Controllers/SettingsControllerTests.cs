using Microsoft.Xna.Framework.Input;
using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Services.Audio;

namespace Somnia.Tests.Controllers;

[TestFixture]
public sealed class SettingsControllerTests
{
    [Test]
    public void DownArrow_AdvancesSelection()
    {
        var ctrl = new SettingsController();
        var state = new SettingsState();
        var audio = new AudioController();

        ctrl.Update(NoKeys(), state, audio);
        ctrl.Update(Keys(Microsoft.Xna.Framework.Input.Keys.Down), state, audio);

        Assert.That(state.SelectedIndex, Is.EqualTo(1));
    }

    [Test]
    public void RightArrow_IncreasesSelectedVolume()
    {
        var ctrl = new SettingsController();
        var state = new SettingsState { SelectedIndex = 1 };
        var audio = new AudioController { MusicVolume = 0.5f };

        for (var i = 0; i < 10; i++)
            ctrl.Update(Keys(Microsoft.Xna.Framework.Input.Keys.Right), state, audio);

        Assert.That(audio.MusicVolume, Is.GreaterThan(0.5f));
    }

    [Test]
    public void Escape_ReturnsBackCommand()
    {
        var ctrl = new SettingsController();
        var state = new SettingsState();
        var audio = new AudioController();

        ctrl.Update(NoKeys(), state, audio);
        var cmd = ctrl.Update(Keys(Microsoft.Xna.Framework.Input.Keys.Escape), state, audio);

        Assert.That(cmd, Is.EqualTo(SettingsController.SettingsCommand.Back));
    }

    private static KeyboardState NoKeys() => new();

    private static KeyboardState Keys(params Microsoft.Xna.Framework.Input.Keys[] keys)
        => new(keys);
}

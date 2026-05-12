using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;

namespace Somnia.Game.Controllers;

/// <summary>
/// Управление главным меню (Title). Превращает edge-нажатия клавиш в команды
/// (Start / Quit / Resume / Restart). Сама фаза в <see cref="Session.GameplaySessionState.UiState"/>.
/// </summary>
public sealed class MenuController
{
    public MenuCommand Update(KeyboardState previous, KeyboardState current, int currentPhase)
    {
        var enter = WasPressed(previous, current, Keys.Enter);
        var space = WasPressed(previous, current, Keys.Space);
        var escape = WasPressed(previous, current, Keys.Escape);
        var quit = WasPressed(previous, current, Keys.Q);
        var settings = WasPressed(previous, current, Keys.O) ||
                       WasPressed(previous, current, Keys.F1);

        switch (currentPhase)
        {
            case GameplayPhase.Title:
                if (enter || space) return MenuCommand.StartNewRun;
                if (settings) return MenuCommand.OpenSettings;
                if (quit) return MenuCommand.Quit;
                break;

            case GameplayPhase.Paused:
                if (escape) return MenuCommand.Resume;
                if (settings) return MenuCommand.OpenSettings;
                if (enter) return MenuCommand.RestartRun;
                if (quit) return MenuCommand.ReturnToTitle;
                break;

            case GameplayPhase.GameOver:
                if (enter || space) return MenuCommand.RestartRun;
                if (escape || quit) return MenuCommand.ReturnToTitle;
                break;
        }

        return MenuCommand.None;
    }

    private static bool WasPressed(KeyboardState previous, KeyboardState current, Keys key) =>
        previous.IsKeyUp(key) && current.IsKeyDown(key);
}

public enum MenuCommand
{
    None,
    StartNewRun,
    Resume,
    RestartRun,
    ReturnToTitle,
    OpenSettings,
    Quit
}

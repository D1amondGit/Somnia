using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.Audio;

namespace Somnia.Game.Controllers;

/// <summary>
/// Управление экраном настроек: Up/Down — выбор пункта; Left/Right — изменение значения;
/// Esc — назад в title; Enter — назад в title (apply implicit).
/// </summary>
public sealed class SettingsController
{
    private const float Step = 0.05f;
    private KeyboardState _prev;

    public enum SettingsCommand
    {
        None,
        Back
    }

    public SettingsCommand Update(KeyboardState ks, SettingsState s, AudioController audio)
    {
        var cmd = SettingsCommand.None;
        if (Pressed(ks, Keys.Escape) || Pressed(ks, Keys.Enter))
            cmd = SettingsCommand.Back;
        if (Pressed(ks, Keys.Up) || Pressed(ks, Keys.W))
            s.MoveUp();
        if (Pressed(ks, Keys.Down) || Pressed(ks, Keys.S))
            s.MoveDown();
        if (Held(ks, Keys.Left) || Held(ks, Keys.A))
            Adjust(s, audio, -Step);
        if (Held(ks, Keys.Right) || Held(ks, Keys.D))
            Adjust(s, audio, +Step);

        _prev = ks;
        return cmd;
    }

    private static void Adjust(SettingsState s, AudioController audio, float delta)
    {
        // Уменьшаем шаг для «зажатой клавиши», иначе слайдер пролетает.
        delta *= 0.18f;
        switch (s.SelectedIndex)
        {
            case 0: audio.MasterVolume += delta; break;
            case 1: audio.MusicVolume += delta; break;
            case 2: audio.SfxVolume += delta; break;
        }
    }

    private bool Pressed(KeyboardState now, Keys key) =>
        now.IsKeyDown(key) && !_prev.IsKeyDown(key);

    private static bool Held(KeyboardState now, Keys key) => now.IsKeyDown(key);
}

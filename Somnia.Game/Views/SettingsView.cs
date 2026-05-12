using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Services.Audio;

namespace Somnia.Game.Views;

/// <summary>
/// Экран настроек: 3 ползунка (Master / Music / SFX). Рендер без внешних спрайтов.
/// </summary>
public sealed class SettingsView
{
    private readonly Texture2D _pixel;

    public SettingsView(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(SpriteBatch sb, SpriteFont? font, int w, int h, SettingsState state, AudioController audio)
    {
        sb.Draw(_pixel, new Rectangle(0, 0, w, h), new Color(6, 8, 12, 230));

        if (font == null) return;

        var title = "SETTINGS";
        var ts = font.MeasureString(SafeText(font, title));
        sb.DrawString(font, SafeText(font, title),
            new Vector2(w / 2f - ts.X * 2f / 2f, h * 0.18f),
            new Color(220, 220, 240), 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

        var labels = new[] { "MASTER", "MUSIC", "SFX" };
        var values = new[] { audio.MasterVolume, audio.MusicVolume, audio.SfxVolume };

        var startY = h * 0.35f;
        for (var i = 0; i < labels.Length; i++)
        {
            var y = startY + i * 70f;
            var selected = i == state.SelectedIndex;
            var color = selected ? new Color(255, 220, 120) : new Color(200, 200, 220);

            var prefix = selected ? "> " : "  ";
            sb.DrawString(font, SafeText(font, $"{prefix}{labels[i]}"),
                new Vector2(w / 2f - 240, y),
                color);

            DrawSlider(sb, (int)(w / 2f - 40), (int)y + 6, 280, 18, values[i], color);

            sb.DrawString(font, SafeText(font, $"{values[i] * 100:F0}%"),
                new Vector2(w / 2f + 256, y),
                color);
        }

        var hint = "Up/Down to select   Left/Right to change   Esc to go back";
        var hs = font.MeasureString(SafeText(font, hint));
        sb.DrawString(font, SafeText(font, hint),
            new Vector2(w / 2f - hs.X / 2f, h * 0.86f),
            new Color(160, 160, 180));
    }

    private void DrawSlider(SpriteBatch sb, int x, int y, int width, int height, float value, Color tint)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        sb.Draw(_pixel, new Rectangle(x, y, width, height), new Color(20, 22, 28));
        sb.Draw(_pixel, new Rectangle(x + 2, y + 2, (int)((width - 4) * value), height - 4), tint);
        for (var i = 1; i < 10; i++)
        {
            var tx = x + width * i / 10;
            sb.Draw(_pixel, new Rectangle(tx, y - 4, 2, height + 8), new Color(60, 60, 70));
        }
    }

    private static string SafeText(SpriteFont font, string text)
    {
        var chars = font.Characters;
        return string.Concat(text.Select(c => chars.Contains(c) ? c : '?'));
    }
}

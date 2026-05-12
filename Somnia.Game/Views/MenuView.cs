using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Somnia.Game.Views;

/// <summary>Главное меню: пульсирующий title и подсказки управления.</summary>
public sealed class MenuView
{
    private readonly Texture2D _pixel;

    public MenuView(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(SpriteBatch sb, SpriteFont? font, int w, int h, double totalSeconds)
    {
        sb.Draw(_pixel, new Rectangle(0, 0, w, h), new Color(8, 10, 14));

        var pulse = 0.5f + 0.5f * (float)System.Math.Sin(totalSeconds * 2.0);
        var titleColor = Color.Lerp(new Color(160, 60, 200), new Color(255, 130, 240), pulse);

        if (font != null)
        {
            DrawCentered(sb, font, "SOMNIA", new Vector2(w / 2f, h * 0.32f), titleColor, scale: 3.4f);
            DrawCentered(sb, font, "An escort under anomalies",
                new Vector2(w / 2f, h * 0.42f), new Color(180, 180, 200), scale: 1.0f);

            DrawCentered(sb, font, "PRESS ENTER TO START",
                new Vector2(w / 2f, h * 0.60f), Color.Lerp(Color.White, Color.LightSkyBlue, pulse), scale: 1.6f);
            DrawCentered(sb, font, "PRESS O FOR SETTINGS",
                new Vector2(w / 2f, h * 0.68f), new Color(200, 200, 220), scale: 1.0f);
            DrawCentered(sb, font, "PRESS Q TO QUIT",
                new Vector2(w / 2f, h * 0.74f), new Color(140, 140, 160), scale: 1.0f);

            DrawCentered(sb, font,
                "WASD MOVE   LMB SKILL   1/2/3 SLOT   SHIFT DASH   E PICKUP   ESC PAUSE",
                new Vector2(w / 2f, h * 0.92f), new Color(100, 100, 120), scale: 0.9f);
        }
        else
        {
            sb.Draw(_pixel, new Rectangle(w / 2 - 220, (int)(h * 0.32f), 440, 60), titleColor);
            sb.Draw(_pixel, new Rectangle(w / 2 - 180, (int)(h * 0.62f), 360, 30),
                Color.Lerp(Color.White, Color.LightSkyBlue, pulse));
        }
    }

    public void DrawPauseOverlay(SpriteBatch sb, SpriteFont? font, int w, int h)
    {
        sb.Draw(_pixel, new Rectangle(0, 0, w, h), Color.Black * 0.55f);
        if (font == null) return;

        DrawCentered(sb, font, "PAUSED", new Vector2(w / 2f, h * 0.40f), Color.White, scale: 2.5f);
        DrawCentered(sb, font, "ESC - RESUME", new Vector2(w / 2f, h * 0.52f), Color.LightGray, scale: 1.2f);
        DrawCentered(sb, font, "ENTER - RESTART", new Vector2(w / 2f, h * 0.57f), Color.LightGray, scale: 1.2f);
        DrawCentered(sb, font, "O - SETTINGS", new Vector2(w / 2f, h * 0.62f), Color.LightGray, scale: 1.2f);
        DrawCentered(sb, font, "Q - TO TITLE", new Vector2(w / 2f, h * 0.67f), Color.LightGray, scale: 1.2f);
    }

    public void DrawGameOverOverlay(SpriteBatch sb, SpriteFont? font, int w, int h, bool playerDead, bool victory)
    {
        sb.Draw(_pixel, new Rectangle(0, 0, w, h), Color.Black * 0.7f);
        if (font == null) return;

        if (victory)
        {
            DrawCentered(sb, font, "RUN COMPLETE", new Vector2(w / 2f, h * 0.4f), Color.Gold, scale: 2.6f);
            DrawCentered(sb, font, "NPC ESCORTED. ANOMALIES SURVIVED.",
                new Vector2(w / 2f, h * 0.48f), new Color(255, 220, 130), scale: 1.0f);
        }
        else
        {
            var label = playerDead ? "YOU DIED" : "NPC LOST";
            DrawCentered(sb, font, label, new Vector2(w / 2f, h * 0.4f), Color.Red, scale: 2.6f);
            DrawCentered(sb, font, "The dream collapses.",
                new Vector2(w / 2f, h * 0.48f), new Color(220, 130, 130), scale: 1.0f);
        }

        DrawCentered(sb, font, "ENTER - TRY AGAIN", new Vector2(w / 2f, h * 0.6f), Color.White, scale: 1.2f);
        DrawCentered(sb, font, "ESC - TO TITLE", new Vector2(w / 2f, h * 0.66f), Color.LightGray, scale: 1.2f);
    }

    private static void DrawCentered(SpriteBatch sb, SpriteFont font, string text, Vector2 center, Color color,
        float scale)
    {
        var safe = SafeText(font, text);
        var size = font.MeasureString(safe) * scale;
        sb.DrawString(font, safe, center - size / 2f, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    /// <summary>Отфильтровать символы, отсутствующие в шрифте, чтобы MonoGame не падал.</summary>
    private static string SafeText(SpriteFont font, string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c == ' ' || font.Characters.Contains(c)) builder.Append(c);
            else if (font.DefaultCharacter.HasValue) builder.Append(font.DefaultCharacter.Value);
        }

        return builder.ToString();
    }
}

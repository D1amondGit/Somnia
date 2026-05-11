using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

/// <summary>Панель статуса и оверлеи паузы/death; «помехи» при раненом NPC.</summary>
public sealed class HudView
{
    private readonly Texture2D _pixel;

    public HudView(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(
        SpriteBatch sb,
        PlayerModel player,
        NpcModel npc,
        SpriteFont? font,
        int bufferWidth,
        int bufferHeight,
        int uiState,
        int arenaDisplayIndex,
        double totalSecondsForGlitch)
    {
        var npcInjured = npc.IsInjured;

        DrawStatusPanel(sb, player, font, npcInjured, totalSecondsForGlitch);

        var uX = bufferWidth - 100;
        var uY = bufferHeight - 120;

        sb.Draw(_pixel, new Rectangle(uX, uY, 60, 60),
            SpritePrimitiveRenderer.ZoneFlashColor(player.CurrentZone));

        float cCd =
            player.ActiveSlot == 0 ? player.Cd1 : player.ActiveSlot == 1 ? player.Cd2 : player.Cd3;

        float mCd =
            player.ActiveSlot == 0 ? player.MaxCd1 :
            player.ActiveSlot == 1 ? player.MaxCd2 : player.MaxCd3;

        var pCd = mCd > 0 ? cCd / mCd : 0f;
        var cdH = (int)(60 * pCd);

        sb.Draw(_pixel, new Rectangle(uX, uY + 60 - cdH, 60, cdH), Color.Black * 0.8f);

        if (npcInjured)
            DrawInterferenceStripe(sb, bufferWidth, bufferHeight, totalSecondsForGlitch);

        if (font == null)
        {
            DrawStateOverlayMinimal(sb, bufferWidth, bufferHeight, uiState);
            return;
        }

        var jitter = npcInjured
            ? new Vector2(
                (float)Math.Sin(totalSecondsForGlitch * 48) * 3f,
                (float)Math.Cos(totalSecondsForGlitch * 37) * 2f)
            : Vector2.Zero;

        sb.DrawString(font, $"ARENA: {arenaDisplayIndex} / 3", new Vector2(uX - 20, uY - 60) + jitter,
            Color.Gold);
        sb.DrawString(font, $"SLOT: {player.ActiveSlot + 1} ZONE: {player.CurrentZone}",
            new Vector2(20, 70) + jitter, Color.White);
        sb.DrawString(font, $"CD: {cCd:F1}s", new Vector2(uX - 10, uY - 30) + jitter, Color.White);

        if (npcInjured)
            sb.DrawString(font, "!!! NPC INJURED - DMG -50% !!!",
                new Vector2(bufferWidth / 2f - 150, 20) + jitter, Color.OrangeRed);

        DrawStateOverlay(sb, font, bufferWidth, bufferHeight, uiState);
    }

    private void DrawStatusPanel(SpriteBatch sb, PlayerModel p, SpriteFont? font, bool glitch, double t)
    {
        var j = glitch
            ? new Vector2((float)Math.Sin(t * 33) * 2f, (float)Math.Cos(t * 29) * 2f)
            : Vector2.Zero;

        sb.Draw(_pixel, new Rectangle(10, 10, 220, 80), Color.Black * 0.7f);
        sb.Draw(_pixel, new Rectangle(20, 20, 200, 20), Color.DarkRed);
        sb.Draw(_pixel, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth / p.MaxHealth)), 20), Color.Red);
        sb.Draw(_pixel, new Rectangle(20, 50, 200, 10), Color.DarkBlue);
        sb.Draw(_pixel, new Rectangle(20, 50, (int)(200 * (p.CurrentMana / p.MaxMana)), 10), Color.Cyan);

        if (font == null) return;
        sb.DrawString(font, $"HP {p.CurrentHealth:F0}", new Vector2(24, 22) + j, Color.White * 0.85f);
        sb.DrawString(font, $"MP {p.CurrentMana:F0}", new Vector2(24, 48) + j, Color.White * 0.85f);
    }

    private void DrawInterferenceStripe(SpriteBatch sb, int w, int h, double t)
    {
        var seed = (int)(t * 30) ^ 0x5f3759df;
        var local = new Random(seed);

        for (var i = 0; i < 42; i++)
        {
            var x = local.Next(-40, w);
            var yy = local.Next(0, h);
            var ww = local.Next(20, 120);
            sb.Draw(_pixel, new Rectangle(x, yy, ww, 2), Color.White * (float)local.NextDouble() * 0.35f);
        }
    }

    private void DrawStateOverlayMinimal(SpriteBatch sb, int w, int h, int uiState)
    {
        if (uiState == 0) return;
        sb.Draw(_pixel, new Rectangle(0, 0, w, h), Color.Black * 0.55f);
    }

    private void DrawStateOverlay(SpriteBatch sb, SpriteFont font, int w, int h, int uiState)
    {
        if (uiState == 0) return;

        sb.Draw(_pixel, new Rectangle(0, 0, w, h), Color.Black * 0.6f);
        switch (uiState)
        {
            case 1:
                sb.DrawString(font, "=== PAUSED ===", new Vector2(w / 2f - 120, h / 2f - 40), Color.White);
                sb.DrawString(font, "Press ESC to Resume", new Vector2(w / 2f - 140, h / 2f), Color.LightGray);
                break;
            default:
                sb.DrawString(font, "=== GAME OVER ===", new Vector2(w / 2f - 130, h / 2f - 40), Color.Red);
                sb.DrawString(font, "Press ENTER to Restart", new Vector2(w / 2f - 155, h / 2f), Color.LightGray);
                break;
        }
    }
}

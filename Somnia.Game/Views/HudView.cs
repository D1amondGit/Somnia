using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

/// <summary>
/// HUD: HP/MP игрока + NPC, большой кругляш активного скилла справа-внизу,
/// 3 слота скиллов внизу-по-центру, индикатор зоны, таймер арены.
/// </summary>
public sealed class HudView
{
    private readonly Texture2D _pixel;
    private readonly SpritePrimitiveRenderer _prim;
    private SkillIconAtlas? _iconAtlas;

    public HudView(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);
        _prim = new SpritePrimitiveRenderer(device);
    }

    /// <summary>Подключить атлас PNG-иконок скиллов. Если null или иконка не загружена — рисуется векторная.</summary>
    public void UseIconAtlas(SkillIconAtlas atlas) => _iconAtlas = atlas;

    public void Draw(
        SpriteBatch sb,
        PlayerModel player,
        NpcModel npc,
        SpriteFont? font,
        int bufferWidth,
        int bufferHeight,
        int arenaDisplayIndex,
        double totalSecondsForGlitch,
        float arenaTimer,
        float arenaTimerMax)
    {
        var npcInjured = npc.IsInjured;

        // HUD-блоки изолированы: краш в одном блоке (например незнакомый символ в шрифте)
        // не должен валить весь кадр.
        Safe(() => DrawPlayerPanel(sb, player, font, npcInjured, totalSecondsForGlitch));
        Safe(() => DrawNpcPanel(sb, npc, font, bufferWidth));
        Safe(() => DrawSkillSlots(sb, player, font, bufferWidth, bufferHeight));
        Safe(() => DrawActiveSkillBigIcon(sb, player, font, bufferWidth, bufferHeight));
        Safe(() => DrawArenaCounter(sb, font, bufferWidth, arenaDisplayIndex));
        Safe(() => DrawZoneBadge(sb, player, font, bufferWidth));
        Safe(() => DrawArenaTimer(sb, font, bufferWidth, arenaTimer, arenaTimerMax, totalSecondsForGlitch));
        Safe(() => DrawBigPixelTimer(sb, bufferHeight, arenaTimer));

        // «Помехи»-полосы по экрану раньше включались при раненом NPC. Отключены —
        // мешали читать UI и сами создавали ощущение «тряски шрифта».
        _ = npcInjured;
        _ = totalSecondsForGlitch;
    }

    private static void Safe(Action a)
    {
        try { a(); } catch { /* HUD-блок упал — не валим весь рендер */ }
    }

    private static string SafeText(SpriteFont? font, string text)
    {
        if (font == null) return text;
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c == ' ' || font.Characters.Contains(c)) sb.Append(c);
            else if (font.DefaultCharacter.HasValue) sb.Append(font.DefaultCharacter.Value);
        }
        return sb.ToString();
    }

    private void DrawPlayerPanel(SpriteBatch sb, PlayerModel p, SpriteFont? font, bool glitch, double t)
    {
        _ = glitch;
        _ = t;

        const int panelW = 280;
        const int panelH = 92;

        sb.Draw(_pixel, new Rectangle(10, 10, panelW, panelH), new Color(8, 10, 14, 220));
        sb.Draw(_pixel, new Rectangle(12, 12, panelW - 4, 2), new Color(60, 70, 100));

        DrawBar(sb, 22, 26, panelW - 24, 18, p.CurrentHealth / p.MaxHealth,
            new Color(50, 15, 20), new Color(230, 60, 70));
        DrawBar(sb, 22, 50, panelW - 24, 10, p.CurrentMana / p.MaxMana,
            new Color(15, 30, 55), new Color(80, 200, 255));
        DrawBar(sb, 22, 66, panelW - 24, 8, p.IsShieldActive ? p.ShieldTimer / 3.5f : 0f,
            new Color(15, 35, 20), new Color(120, 230, 130));

        if (font == null) return;
        // Чёткий читаемый текст без jitter, тень для лучшего контраста.
        DrawTextWithShadow(sb, font, SafeText(font, $"HP {p.CurrentHealth:F0}/{p.MaxHealth:F0}"),
            new Vector2(24, 26), Color.White);
        DrawTextWithShadow(sb, font, SafeText(font, $"MP {p.CurrentMana:F0}"),
            new Vector2(24, 50), new Color(220, 230, 245));
    }

    private void DrawTextWithShadow(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color)
    {
        sb.DrawString(font, text, pos + new Vector2(1, 1), Color.Black * 0.7f);
        sb.DrawString(font, text, pos, color);
    }

    private void DrawNpcPanel(SpriteBatch sb, NpcModel npc, SpriteFont? font, int bufferWidth)
    {
        const int panelW = 260;
        const int panelH = 60;
        var x = bufferWidth / 2 - panelW / 2;
        const int y = 10;

        sb.Draw(_pixel, new Rectangle(x, y, panelW, panelH), new Color(8, 10, 14, 220));
        sb.Draw(_pixel, new Rectangle(x + 2, y + 2, panelW - 4, 2), new Color(120, 100, 50));

        var color = npc.IsInjured ? new Color(255, 130, 80) : new Color(120, 230, 130);
        DrawBar(sb, x + 12, y + 22, panelW - 24, 16, npc.Health / npc.MaxHealth,
            new Color(30, 20, 10), color);

        if (font == null) return;
        var label = npc.IsPickedUp ? "NPC (carried)" : "NPC";
        sb.DrawString(font, SafeText(font, label), new Vector2(x + 14, y + 22), Color.White * 0.85f);
    }

    private void DrawSkillSlots(SpriteBatch sb, PlayerModel p, SpriteFont? font, int bufferWidth, int bufferHeight)
    {
        const int slotW = 78;
        const int slotH = 78;
        const int spacing = 14;
        var totalW = slotW * 3 + spacing * 2;
        var startX = bufferWidth / 2 - totalW / 2;
        var y = bufferHeight - slotH - 28;

        var zoneTint = PlayerPalette.GetZoneTint(p.CurrentZone);

        for (var slot = 0; slot < 3; slot++)
        {
            var x = startX + slot * (slotW + spacing);
            var (cur, max) = SlotCooldown(p, slot);
            var ready = cur <= 0;
            var pct = max > 0 ? cur / max : 0f;
            var icon = SkillSlotCatalog.Get(p.CurrentZone, slot);

            sb.Draw(_pixel, new Rectangle(x, y, slotW, slotH), new Color(10, 12, 18, 230));

            var border = slot == p.ActiveSlot ? icon.Tint : new Color(60, 60, 70);
            DrawBorder(sb, x, y, slotW, slotH, 2, border);

            var iconCenter = new Vector2(x + slotW / 2f, y + slotH / 2f + 4);
            SkillIconView.DrawIcon(sb, _prim, _pixel, icon.Icon, iconCenter, slotW * 0.85f,
                ready ? icon.Tint : icon.Tint * 0.35f, _iconAtlas);

            if (!ready)
            {
                var cdH = (int)(slotH * pct);
                sb.Draw(_pixel, new Rectangle(x + 2, y + slotH - cdH, slotW - 4, cdH),
                    Color.Black * 0.6f);
            }

            if (font != null)
            {
                var key = (slot + 1).ToString();
                sb.DrawString(font, SafeText(font, key), new Vector2(x + 6, y + 4), border);
                if (!ready)
                    sb.DrawString(font, SafeText(font, $"{cur:F1}s"),
                        new Vector2(x + slotW / 2f - 18, y + slotH - 22), Color.White);
            }
        }
    }

    /// <summary>
    /// Кругляш активного скилла. Стоит в правом ВЕРХНЕМ углу, чтобы не перекрывать
    /// нижний правый сектор арены — игрок должен иметь возможность стрелять туда без помех.
    /// </summary>
    private void DrawActiveSkillBigIcon(SpriteBatch sb, PlayerModel p, SpriteFont? font,
        int bufferWidth, int bufferHeight)
    {
        const int radius = 52;
        var center = new Vector2(bufferWidth - radius - 24, radius + 26);
        _ = bufferHeight;

        var icon = SkillSlotCatalog.Get(p.CurrentZone, p.ActiveSlot);
        var (cur, max) = SlotCooldown(p, p.ActiveSlot);
        var ready = cur <= 0f;
        var pct = max > 0 ? MathHelper.Clamp(cur / max, 0f, 1f) : 0f;

        _prim.DrawCircleOutline(sb, center, radius + 12f, new Color(30, 34, 42) * 0.8f, 3);
        FillCircle(sb, center, radius + 6f, new Color(10, 12, 18) * 0.85f);

        var ringColor = ready ? icon.Tint : icon.Tint * 0.45f;
        _prim.DrawCircleOutline(sb, center, radius, ringColor, 4);
        SkillIconView.DrawIcon(sb, _prim, _pixel, icon.Icon, center, radius * 2f * 0.85f, ringColor, _iconAtlas);

        if (!ready)
        {
            // «пирожок» оставшегося кулдауна
            DrawCooldownArc(sb, center, radius - 8f, pct);
        }

        if (font == null) return;
        var label = SafeText(font, icon.Title);
        var size = font.MeasureString(label);
        sb.DrawString(font, label,
            new Vector2(center.X - size.X / 2f, center.Y + radius + 6),
            Color.White * 0.85f);
    }

    private void DrawCooldownArc(SpriteBatch sb, Vector2 center, float radius, float pct)
    {
        const int segments = 40;
        var filled = (int)(segments * pct);
        for (var i = 0; i < filled; i++)
        {
            var ang = -MathHelper.PiOver2 + i * (MathHelper.TwoPi / segments);
            var p1 = center + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * (radius - 5);
            var p2 = center + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * radius;
            _prim.DrawLine(sb, p1, p2, Color.Black * 0.85f, 5);
        }
    }

    private void FillCircle(SpriteBatch sb, Vector2 center, float radius, Color color)
    {
        const int segments = 28;
        var verts = new System.Collections.Generic.List<Vector2>(segments);
        for (var i = 0; i < segments; i++)
        {
            var a = i * MathHelper.TwoPi / segments;
            verts.Add(center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * radius);
        }
        _prim.FillPoly(sb, verts, color);
    }

    private static (float Current, float Max) SlotCooldown(PlayerModel p, int slot) =>
        slot switch
        {
            0 => (p.Cd1, p.MaxCd1),
            1 => (p.Cd2, p.MaxCd2),
            2 => (p.Cd3, p.MaxCd3),
            _ => (0f, 1f)
        };

    private void DrawArenaCounter(SpriteBatch sb, SpriteFont? font, int bufferWidth, int arenaDisplayIndex)
    {
        if (font == null) return;
        var text = arenaDisplayIndex < 0
            ? SafeText(font, "SECRET: MEAT GRINDER")
            : SafeText(font, $"ARENA {arenaDisplayIndex} / 3");
        // Чёткий текст без scale-размытия — рисуем native size.
        var size = font.MeasureString(text);
        var pos = new Vector2(bufferWidth / 2f - size.X / 2f, 76);
        sb.DrawString(font, text, pos + new Vector2(1, 1), Color.Black * 0.7f);
        sb.DrawString(font, text, pos, new Color(255, 210, 80));
    }

    private void DrawZoneBadge(SpriteBatch sb, PlayerModel p, SpriteFont? font, int bufferWidth)
    {
        var color = PlayerPalette.GetZoneTint(p.CurrentZone);
        var x = 14;
        var y = 110;
        sb.Draw(_pixel, new Rectangle(x, y, 180, 28), new Color(8, 10, 14, 220));
        sb.Draw(_pixel, new Rectangle(x, y, 6, 28), color);
        if (font == null) return;
        var text = SafeText(font, $"ZONE: {p.CurrentZone}");
        var pos = new Vector2(x + 12, y + 4);
        sb.DrawString(font, text, pos + new Vector2(1, 1), Color.Black * 0.7f);
        sb.DrawString(font, text, pos, color);
    }

    private void DrawArenaTimer(SpriteBatch sb, SpriteFont? font, int bufferWidth,
        float timer, float maxTimer, double tt)
    {
        if (maxTimer <= 0f) return;
        var safe = MathHelper.Clamp(timer, 0f, maxTimer);
        var pct = safe / maxTimer;

        var x = bufferWidth / 2 - 220;
        var y = 78;

        sb.Draw(_pixel, new Rectangle(x, y, 440, 14), new Color(8, 10, 14, 220));
        var color = timer <= 0
            ? Color.Lerp(new Color(180, 30, 50), Color.White, 0.5f + 0.5f * (float)Math.Sin(tt * 8))
            : timer < 15f
                ? Color.Lerp(new Color(240, 150, 60), new Color(220, 60, 70), 1f - pct)
                : new Color(220, 220, 240);
        sb.Draw(_pixel, new Rectangle(x + 2, y + 2, (int)((440 - 4) * pct), 10), color);

        if (font == null) return;
        var label = SafeText(font, timer <= 0
            ? "OVERTIME - danger rising"
            : $"TIME {timer:F0}s");
        var size = font.MeasureString(label) * 0.9f;
        sb.DrawString(font, label,
            new Vector2(bufferWidth / 2f - size.X / 2f, y + 18),
            color, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
    }

    private void DrawBar(SpriteBatch sb, int x, int y, int w, int h, float fraction, Color bg, Color fg)
    {
        fraction = MathHelper.Clamp(fraction, 0f, 1f);
        sb.Draw(_pixel, new Rectangle(x, y, w, h), bg);
        sb.Draw(_pixel, new Rectangle(x, y, (int)(w * fraction), h), fg);
    }

    private void DrawBorder(SpriteBatch sb, int x, int y, int w, int h, int thickness, Color color)
    {
        sb.Draw(_pixel, new Rectangle(x, y, w, thickness), color);
        sb.Draw(_pixel, new Rectangle(x, y + h - thickness, w, thickness), color);
        sb.Draw(_pixel, new Rectangle(x, y, thickness, h), color);
        sb.Draw(_pixel, new Rectangle(x + w - thickness, y, thickness, h), color);
    }

    /// <summary>
    /// Большой битмапный таймер арены в левом нижнем углу. Формат MM:SS,
    /// в овертайме показывает мигающее +SS (сколько прошло за чертой).
    /// </summary>
    private void DrawBigPixelTimer(SpriteBatch sb, int bufferHeight, float arenaTimer)
    {
        const int pixelSize = 7;
        const int pixelSpacing = 1;

        string text;
        Color color;
        if (arenaTimer > 0)
        {
            var total = (int)System.Math.Ceiling(arenaTimer);
            var minutes = total / 60;
            var seconds = total % 60;
            text = $"{minutes:0}:{seconds:00}";
            color = arenaTimer < 15f
                ? new Color(255, 120, 80)
                : new Color(220, 230, 255);
        }
        else
        {
            text = "0:00";
            color = new Color(255, 70, 70);
        }

        var width = BigPixelDigit.MeasureWidth(text, pixelSize, pixelSpacing);
        var height = BigPixelDigit.MeasureHeight(pixelSize, pixelSpacing);
        var x = 20f;
        var y = bufferHeight - height - 22f;

        // Лёгкая подложка под цифры.
        sb.Draw(_pixel,
            new Rectangle((int)x - 12, (int)y - 12, width + 24, height + 24),
            new Color(6, 8, 12, 180));

        // «Тень»
        BigPixelDigit.Draw(sb, _pixel, text,
            new Vector2(x + 3, y + 3),
            new Color(0, 0, 0, 200), pixelSize, pixelSpacing);
        BigPixelDigit.Draw(sb, _pixel, text, new Vector2(x, y), color, pixelSize, pixelSpacing);
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
}

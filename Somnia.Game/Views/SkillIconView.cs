using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

/// <summary>
/// Векторные иконки скиллов — никаких внешних спрайтов, всё рисуется через SpritePrimitiveRenderer.
/// Иконки одной формы (<see cref="SkillIconShape"/>) одинаково выглядят и в большом круге, и в мелком слоте.
/// </summary>
public static class SkillIconView
{
    public static void DrawIcon(SpriteBatch sb, SpritePrimitiveRenderer prim, Texture2D pixel,
        SkillIconShape shape, Vector2 center, float size, Color color,
        SkillIconAtlas? atlas = null)
    {
        // Если есть PNG-текстура для этого скилла — рисуем её и выходим.
        var tex = atlas?.Get(shape);
        if (tex != null)
        {
            var s = (int)size;
            sb.Draw(tex, new Rectangle((int)center.X - s / 2, (int)center.Y - s / 2, s, s),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            return;
        }

        switch (shape)
        {
            case SkillIconShape.Rifle:
                DrawRifle(sb, pixel, center, size, color);
                break;
            case SkillIconShape.Shotgun:
                DrawShotgun(sb, pixel, center, size, color);
                break;
            case SkillIconShape.Sniper:
                DrawSniper(sb, pixel, center, size, color);
                break;
            case SkillIconShape.Grenade:
                DrawGrenade(sb, prim, pixel, center, size, color);
                break;
            case SkillIconShape.Aura:
                DrawAura(sb, prim, center, size, color);
                break;
            case SkillIconShape.Dash:
                DrawDash(sb, pixel, center, size, color);
                break;
            case SkillIconShape.Bomb:
                DrawBomb(sb, prim, center, size, color);
                break;
            case SkillIconShape.Slow:
                DrawSlow(sb, prim, pixel, center, size, color);
                break;
            case SkillIconShape.Pull:
                DrawPull(sb, pixel, center, size, color);
                break;
            case SkillIconShape.Infect:
                DrawInfect(sb, prim, pixel, center, size, color);
                break;
            case SkillIconShape.None:
            default:
                DrawDash(sb, pixel, center, size, color * 0.4f);
                break;
        }
    }

    private static void DrawRifle(SpriteBatch sb, Texture2D pixel, Vector2 c, float s, Color col)
    {
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.45f), (int)(c.Y - s * 0.07f),
            (int)(s * 0.9f), (int)(s * 0.14f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.5f), (int)(c.Y - s * 0.18f),
            (int)(s * 0.15f), (int)(s * 0.18f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X + s * 0.30f), (int)(c.Y - s * 0.04f),
            (int)(s * 0.18f), (int)(s * 0.08f)), col * 0.7f);
    }

    private static void DrawShotgun(SpriteBatch sb, Texture2D pixel, Vector2 c, float s, Color col)
    {
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.45f), (int)(c.Y - s * 0.10f),
            (int)(s * 0.9f), (int)(s * 0.07f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.45f), (int)(c.Y + s * 0.03f),
            (int)(s * 0.9f), (int)(s * 0.07f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.5f), (int)(c.Y - s * 0.18f),
            (int)(s * 0.18f), (int)(s * 0.36f)), col * 0.8f);
    }

    private static void DrawSniper(SpriteBatch sb, Texture2D pixel, Vector2 c, float s, Color col)
    {
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.50f), (int)(c.Y - s * 0.04f),
            (int)(s * 1.0f), (int)(s * 0.08f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.12f), (int)(c.Y - s * 0.20f),
            (int)(s * 0.10f), (int)(s * 0.10f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X + s * 0.30f), (int)(c.Y - s * 0.03f),
            (int)(s * 0.20f), (int)(s * 0.06f)), col * 0.7f);
    }

    private static void DrawGrenade(SpriteBatch sb, SpritePrimitiveRenderer prim, Texture2D pixel,
        Vector2 c, float s, Color col)
    {
        prim.DrawCircleOutline(sb, c, s * 0.30f, col, 3);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.06f), (int)(c.Y - s * 0.42f),
            (int)(s * 0.12f), (int)(s * 0.12f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.16f), (int)(c.Y - s * 0.34f),
            (int)(s * 0.10f), (int)(s * 0.06f)), col * 0.6f);
    }

    private static void DrawAura(SpriteBatch sb, SpritePrimitiveRenderer prim, Vector2 c, float s, Color col)
    {
        prim.DrawCircleOutline(sb, c, s * 0.42f, col, 3);
        prim.DrawCircleOutline(sb, c, s * 0.30f, col * 0.7f, 2);
        prim.DrawCircleOutline(sb, c, s * 0.18f, col * 0.5f, 2);
    }

    private static void DrawDash(SpriteBatch sb, Texture2D pixel, Vector2 c, float s, Color col)
    {
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.45f), (int)(c.Y - s * 0.03f),
            (int)(s * 0.4f), (int)(s * 0.06f)), col * 0.4f);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.10f), (int)(c.Y - s * 0.05f),
            (int)(s * 0.55f), (int)(s * 0.10f)), col);
    }

    private static void DrawBomb(SpriteBatch sb, SpritePrimitiveRenderer prim, Vector2 c, float s, Color col)
    {
        prim.DrawCircleOutline(sb, c, s * 0.40f, col, 4);
        prim.DrawCircleOutline(sb, c, s * 0.20f, col * 0.5f, 2);
    }

    private static void DrawSlow(SpriteBatch sb, SpritePrimitiveRenderer prim, Texture2D pixel,
        Vector2 c, float s, Color col)
    {
        prim.DrawCircleOutline(sb, c, s * 0.42f, col * 0.7f, 2);
        for (var i = 0; i < 6; i++)
        {
            var ang = i * (MathHelper.TwoPi / 6f);
            var p = c + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * s * 0.30f;
            sb.Draw(pixel, new Rectangle((int)(p.X - 1), (int)(p.Y - 1), 3, 3), col);
        }
    }

    private static void DrawPull(SpriteBatch sb, Texture2D pixel, Vector2 c, float s, Color col)
    {
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.40f), (int)(c.Y - s * 0.05f),
            (int)(s * 0.30f), (int)(s * 0.10f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X - s * 0.10f), (int)(c.Y - s * 0.20f),
            (int)(s * 0.20f), (int)(s * 0.40f)), col);
        sb.Draw(pixel, new Rectangle((int)(c.X + s * 0.15f), (int)(c.Y - s * 0.05f),
            (int)(s * 0.30f), (int)(s * 0.10f)), col * 0.5f);
    }

    private static void DrawInfect(SpriteBatch sb, SpritePrimitiveRenderer prim, Texture2D pixel,
        Vector2 c, float s, Color col)
    {
        prim.DrawCircleOutline(sb, c, s * 0.36f, col, 3);
        sb.Draw(pixel, new Rectangle((int)(c.X - 2), (int)(c.Y - 2), 4, 4), col);
        var p1 = c + new Vector2(s * 0.20f, s * 0.20f);
        var p2 = c + new Vector2(-s * 0.20f, s * 0.10f);
        sb.Draw(pixel, new Rectangle((int)(p1.X - 1), (int)(p1.Y - 1), 3, 3), col * 0.8f);
        sb.Draw(pixel, new Rectangle((int)(p2.X - 1), (int)(p2.Y - 1), 3, 3), col * 0.8f);
    }
}

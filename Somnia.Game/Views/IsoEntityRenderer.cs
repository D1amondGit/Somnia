using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

/// <summary>
/// Рендерит сущности (игрок/NPC/враги) как маленькие изометрические колонки — гекс-основание,
/// три видимых стенки, верхушка. Так они вписываются в наклонный мир, а не выглядят 2D-квадратами.
/// </summary>
public static class IsoEntityRenderer
{
    public static void DrawCharacter(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 footPosition,
        float baseRadius,
        float height,
        Color body,
        Color accent)
    {
        DrawShadow(sb, prim, footPosition, baseRadius);

        var hex = new HexagonModel(footPosition, baseRadius, height,
            IsometricView.Squash, IsometricView.Tilt);

        var baseV = hex.GetBaseVertices();
        var topV = hex.GetTopVertices();

        // Стенки колонки: три «фронтальных» грани (с положительной Y у базы)
        var dark = MultiplyColor(body, 0.45f);
        for (var i = 0; i < 6; i++)
        {
            var a = baseV[i];
            var b = baseV[(i + 1) % 6];
            if (a.Y + b.Y < footPosition.Y * 2f) continue; // не рисуем тыльные стенки

            var quad = new List<Vector2>
            {
                a, b,
                topV[(i + 1) % 6], topV[i]
            };
            prim.FillPoly(sb, quad, dark);
            DrawOutline(sb, prim, quad, MultiplyColor(body, 0.25f));
        }

        prim.FillPoly(sb, topV, body);
        DrawOutline(sb, prim, topV, accent);
    }

    public static void DrawHealthBar(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 footPosition,
        float characterHeight,
        float widthPx,
        float fraction,
        Color barColor)
    {
        fraction = MathHelper.Clamp(fraction, 0f, 1f);
        var top = footPosition.Y - characterHeight - 14f;
        var x = footPosition.X - widthPx / 2f;

        sb.Draw(prim.PixelTexture,
            new Rectangle((int)x - 1, (int)top - 1, (int)widthPx + 2, 6),
            new Color(0, 0, 0, 180));
        sb.Draw(prim.PixelTexture,
            new Rectangle((int)x, (int)top, (int)(widthPx * fraction), 4),
            barColor);
    }

    public static void DrawTelegraphLine(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 from,
        Vector2 toward,
        float length,
        Color color)
    {
        var dir = toward - from;
        if (dir == Vector2.Zero) return;
        dir.Normalize();
        prim.DrawLine(sb, from, from + dir * length, color, thickness: 2);
    }

    /// <summary>
    /// Длинный прицельный «лазер» снайпера: мягкое свечение в основе, пунктир
    /// сверху, маленький прицел-«глаз» в конечной точке. Куда стрельнёт — туда и
    /// рисуется (вектор <paramref name="to"/>).
    /// </summary>
    public static void DrawLaserSight(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 from,
        Vector2 to,
        Color color,
        double timeSec)
    {
        var dir = to - from;
        var len = dir.Length();
        if (len < 1f) return;
        dir /= len;

        // Свечение «вкруг линии» — несколько прозрачных слоёв.
        prim.DrawLine(sb, from, to, color * 0.18f, thickness: 7);
        prim.DrawLine(sb, from, to, color * 0.45f, thickness: 3);

        // Пунктир сверху — даёт ощущение «трассировки».
        const float dash = 18f;
        const float gap = 12f;
        var t = (float)(timeSec * 220.0 % (dash + gap));
        var travel = -t;
        while (travel < len)
        {
            var s1 = MathHelper.Max(0f, travel);
            var s2 = MathHelper.Min(len, travel + dash);
            if (s2 > s1)
                prim.DrawLine(sb, from + dir * s1, from + dir * s2, color, thickness: 1);
            travel += dash + gap;
        }

        // Прицел-крестик в конечной точке.
        var perp = new Vector2(-dir.Y, dir.X);
        var hit = to;
        const float reticle = 9f;
        prim.DrawLine(sb, hit - dir * reticle, hit + dir * reticle, color, thickness: 1);
        prim.DrawLine(sb, hit - perp * reticle, hit + perp * reticle, color, thickness: 1);
        prim.DrawCircleOutline(sb, hit, reticle * 0.5f, color, 1);
    }

    /// <summary>
    /// Telegraph-окружность AoE-атаки (slam босса, граната). Заполняет область
    /// полупрозрачной заливкой и рисует пульсирующий контур.
    /// </summary>
    public static void DrawAoeTelegraph(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 center,
        float radius,
        Color color,
        float progress01)
    {
        var pulse = 0.5f + 0.5f * (float)System.Math.Sin(progress01 * System.Math.PI * 12);
        prim.DrawCircleOutline(sb, center, radius, color * (0.65f + pulse * 0.35f), 4);
        prim.DrawCircleOutline(sb, center, radius * 0.55f, color * 0.4f, 2);
    }

    /// <summary>Muzzle flash: лёгкий «пушок» в сторону <paramref name="dir"/> над врагом.</summary>
    public static void DrawMuzzleFlash(
        SpriteBatch sb,
        SpritePrimitiveRenderer prim,
        Vector2 origin,
        Vector2 dir,
        float strength01,
        Color color)
    {
        if (strength01 <= 0f) return;
        if (dir == Vector2.Zero) dir = Vector2.UnitY;
        else dir.Normalize();

        var len = 30f * strength01 + 16f;
        var end = origin + dir * len;
        prim.DrawLine(sb, origin, end, color * (0.8f * strength01), thickness: 5);
        prim.DrawCircleOutline(sb, origin, 9f * strength01 + 4f, color * strength01, 3);
        prim.DrawCircleOutline(sb, end, 5f * strength01 + 2f, Color.White * strength01, 2);
    }

    private static void DrawShadow(SpriteBatch sb, SpritePrimitiveRenderer prim, Vector2 foot, float radius)
    {
        var shadow = new HexagonModel(foot, radius * 1.15f, 0f,
            IsometricView.Squash * 0.65f, IsometricView.Tilt * 0.4f);
        prim.FillPoly(sb, shadow.GetBaseVertices(), new Color(0, 0, 0, 110));
    }

    private static void DrawOutline(SpriteBatch sb, SpritePrimitiveRenderer prim,
        IReadOnlyList<Vector2> verts, Color color)
    {
        for (var i = 0; i < verts.Count; i++)
            prim.DrawLine(sb, verts[i], verts[(i + 1) % verts.Count], color, thickness: 2);
    }

    private static Color MultiplyColor(Color c, float k) =>
        new(
            (byte)(c.R * k),
            (byte)(c.G * k),
            (byte)(c.B * k),
            c.A);
}

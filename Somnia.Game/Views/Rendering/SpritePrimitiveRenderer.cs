using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;

namespace Somnia.Game.Views.Rendering;

public sealed class SpritePrimitiveRenderer
{
    private readonly Texture2D _tex;

    public SpritePrimitiveRenderer(GraphicsDevice gd)
    {
        _tex = new Texture2D(gd, 1, 1);
        _tex.SetData([Color.White]);
    }

    public Texture2D PixelTexture => _tex;

    public void FillPoly(SpriteBatch sb, IReadOnlyList<Vector2> vertices, Color color)
    {
        if (vertices.Count < 3) return;

        float minY = vertices.Min(p => p.Y);
        float maxY = vertices.Max(p => p.Y);

        for (var y = minY; y <= maxY; y += 1f)
        {
            var nodes = new List<float>();

            for (var i = 0; i < vertices.Count; i++)
            {
                var p1 = vertices[i];
                var p2 = vertices[(i + 1) % vertices.Count];
                if (!(p1.Y < y && p2.Y >= y) && !(p2.Y < y && p1.Y >= y))
                    continue;

                var x = p1.X + (y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X);
                nodes.Add(x);
            }

            nodes.Sort();
            for (var n = 0; n + 1 < nodes.Count; n += 2)
                sb.Draw(_tex, new Rectangle((int)nodes[n], (int)y, (int)(nodes[n + 1] - nodes[n] + 1), 2), color);
        }
    }

    /// <summary>
    /// Чуть вытянуть текстуру по высоте грани (визуально крупнее кладка на торце).
    /// </summary>
    public const float WallFaceHeightScale = 1.12f;

    /// <summary>Основная модуляция боковой грани — тёмный «объём тени» под чёрной шапкой гекса.</summary>
    public static readonly Color WallFaceShadowModulate = new Color(22, 24, 30, 255);

    /// <summary>Второй проход: лёгкий lift по альфе (зернистость), не осветлять сильно — стены остаются тёмными.</summary>
    public static readonly Color WallFaceGrainModulate = new Color(255, 255, 255, 18);

    /// <summary>
    /// Одна боковая грань призмы: верх t1→t2, низ b1→b2.
    /// В нашей изометрии рёбра t1–b1 и t2–b2 вертикальны в экране, а верх/низ параллельны — это трапеция,
    /// а не параллелограмм: один <see cref="SpriteBatch.Draw"/> с rotation не может её покрыть.
    /// Режем грань вертикальными полосками по X: каждая — ось-выровненный прямоугольник с корректным U по текстуре.
    /// </summary>
    public void DrawWallFace(
        SpriteBatch sb,
        Texture2D? texture,
        Vector2 t1,
        Vector2 t2,
        Vector2 b1,
        Vector2 b2,
        float wallHeightFallback,
        float faceHeightScale = WallFaceHeightScale)
    {
        if (texture == null) return;

        var texW = texture.Width;
        var texH = texture.Height;
        if (texW < 1 || texH < 1) return;

        var x1 = t1.X;
        var x2 = t2.X;
        var denom = x2 - x1;
        const int stripStep = 2;
        var minX = (int)MathHelper.Min(MathHelper.Min(t1.X, t2.X), MathHelper.Min(b1.X, b2.X));
        var maxX = (int)Math.Ceiling(MathHelper.Max(MathHelper.Max(t1.X, t2.X), MathHelper.Max(b1.X, b2.X)));
        if (maxX <= minX)
            maxX = minX + stripStep;

        for (var xi = minX; xi < maxX; xi += stripStep)
        {
            var xc = xi + stripStep * 0.5f;
            float s;
            if (MathF.Abs(denom) < 0.25f)
                s = 0.5f;
            else
                s = MathHelper.Clamp((xc - x1) / denom, 0f, 1f);

            var topPt = Vector2.Lerp(t1, t2, s);
            var botPt = Vector2.Lerp(b1, b2, s);
            var rawH = botPt.Y - topPt.Y;
            var y0 = rawH >= 0f ? topPt.Y : botPt.Y;
            var absH = MathF.Abs(rawH);
            if (absH < 1f)
                absH = wallHeightFallback * faceHeightScale;
            else
                absH *= faceHeightScale;

            var destH = MathHelper.Max(1, (int)MathF.Ceiling(absH));
            var destRect = new Rectangle(xi, (int)MathF.Floor(y0), stripStep, destH);

            var srcX = (int)(s * MathHelper.Max(0, texW - 1));
            var srcRect = new Rectangle(srcX, 0, 1, texH);

            sb.Draw(texture, destRect, srcRect, WallFaceShadowModulate);
            sb.Draw(texture, destRect, srcRect, WallFaceGrainModulate);
        }
    }

    public static void DrawHexWalls(SpriteBatch sb, SpritePrimitiveRenderer prim, HexagonModel hex, Texture2D? wallTex)
    {
        if (wallTex == null) return;
        var top = hex.GetTopVertices();
        var bas = hex.GetBaseVertices();
        if (top.Count < 6 || bas.Count < 6) return;

        for (var i = 0; i < 6; i++)
        {
            prim.DrawWallFace(sb, wallTex,
                top[i], top[(i + 1) % 6],
                bas[i], bas[(i + 1) % 6],
                hex.WallHeight);
        }
    }

    public void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c, int thickness = 2)
    {
        var e = p2 - p1;
        var a = (float)Math.Atan2(e.Y, e.X);

        sb.Draw(_tex, new Rectangle((int)p1.X, (int)p1.Y, (int)e.Length(), thickness), null, c, a, new Vector2(0, 0.5f), 0,
            0);
    }

    public void DrawCircleOutline(SpriteBatch sb, Vector2 center, float radius, Color c, int thickness = 2)
    {
        float inc = MathHelper.TwoPi / 32f;
        var th = 0f;
        var p1 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * radius;
        for (var i = 0; i < 32; i++)
        {
            th += inc;
            var p2 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * radius;
            DrawLine(sb, p1, p2, c, thickness);
            p1 = p2;
        }
    }

    public void DrawCone(SpriteBatch sb, Vector2 origin, Vector2 direction, float radius, float cosHalfAngle, Color col)
    {
        float bA = (float)Math.Atan2(direction.Y, direction.X);
        var s = MathF.Acos(MathHelper.Clamp(cosHalfAngle, -1f, 1f));

        var p1 = origin + new Vector2(MathF.Cos(bA - s), MathF.Sin(bA - s)) * radius;
        var p2 = origin + new Vector2(MathF.Cos(bA + s), MathF.Sin(bA + s)) * radius;

        DrawLine(sb, origin, p1, col, 3);
        DrawLine(sb, origin, p2, col, 3);
        DrawLine(sb, p1, origin + direction * radius, col, 2);
        DrawLine(sb, origin + direction * radius, p2, col, 2);
    }

    public static Color ZoneFlashColor(AnomalyType t) =>
        t switch
        {
            AnomalyType.Red => Color.Red,
            AnomalyType.Blue => Color.Blue,
            AnomalyType.Green => Color.Green,
            AnomalyType.Neutral => Color.Gray,
            _ => Color.White
        };
}

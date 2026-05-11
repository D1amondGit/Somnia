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

    public void DrawWall(SpriteBatch sb, Texture2D? texture, Vector2 p1, Vector2 p2, float wallHeight)
    {
        if (texture == null) return;
        if (p1.X > p2.X) (p1, p2) = (p2, p1);

        var w = p2.X - p1.X;
        if (w <= 0) return;

        for (var x = 0f; x <= w; x++)
        {
            var ty = MathHelper.Lerp(p1.Y, p2.Y, x / Math.Max(w, 0.001f));
            sb.Draw(texture, new Rectangle((int)(p1.X + x), (int)ty, 2, (int)wallHeight), Color.Gray);
        }
    }

    public static void DrawHexWalls(SpriteBatch sb, SpritePrimitiveRenderer prim, HexagonModel hex, Texture2D? wallTex)
    {
        if (wallTex == null) return;
        var r = hex.GetTopVertices();

        prim.DrawWall(sb, wallTex, r.ElementAt(0), r.ElementAt(1), hex.WallHeight);
        prim.DrawWall(sb, wallTex, r.ElementAt(1), r.ElementAt(2), hex.WallHeight);
        prim.DrawWall(sb, wallTex, r.ElementAt(2), r.ElementAt(3), hex.WallHeight);
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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Services.World;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views.Floor;

/// <summary>Пол из крупных гексов, согласованных с ArenaHexGrid. Без процедурного шума.</summary>
public static class LargeHexFloorRenderer
{
    public static void Draw(SpriteBatch sb, SpritePrimitiveRenderer prim, Rectangle playArea, int layoutSeed)
    {
        var origin = ArenaHexGrid.GetOrigin(layoutSeed);

        var margin = ArenaHexGrid.CircumRadius * 3f;
        var minX = playArea.Left - margin;
        var maxX = playArea.Right + margin;
        var minY = playArea.Top - margin;
        var maxY = playArea.Bottom + margin;

        var rMin = (int)Math.Floor((minY - origin.Y) / ArenaHexGrid.VerticalSpacing) - 2;
        var rMax = (int)Math.Ceiling((maxY - origin.Y) / ArenaHexGrid.VerticalSpacing) + 2;
        var qMin = (int)Math.Floor((minX - origin.X) / ArenaHexGrid.HorizontalSpacing) - 3;
        var qMax = (int)Math.Ceiling((maxX - origin.X) / ArenaHexGrid.HorizontalSpacing) + 3;

        var rndTint = new Random(layoutSeed ^ 0x00BEEF);

        for (var r = rMin; r <= rMax; r++)
        {
            for (var q = qMin; q <= qMax; q++)
            {
                var center = ArenaHexGrid.CellCenter(q, r, origin);
                if (center.X < minX || center.X > maxX || center.Y < minY || center.Y > maxY)
                    continue;

                var fill = PickFloorColor(q, r, rndTint);
                var tile = new HexagonModel(
                    center,
                    ArenaHexGrid.CircumRadius,
                    wallHeight: 0f,
                    squash: ArenaHexGrid.Squash,
                    tilt: ArenaHexGrid.Tilt);

                var verts = tile.GetBaseVertices();
                prim.FillPoly(sb, verts, fill);

                var outline = new Color(52, 58, 68) * (0.35f + (float)(rndTint.NextDouble() * 0.08));
                DrawHexOutline(sb, prim, verts, outline);
            }
        }
    }

    private static Color PickFloorColor(int q, int r, Random rndTint)
    {
        var checker = (q + r) & 1;
        var baseDark = checker == 0 ? new Color(18, 20, 24) : new Color(26, 29, 35);
        var jitter = rndTint.Next(-4, 5);
        return new Color(
            Math.Clamp(baseDark.R + jitter, 8, 50),
            Math.Clamp(baseDark.G + jitter, 8, 55),
            Math.Clamp(baseDark.B + jitter, 8, 60));
    }

    private static void DrawHexOutline(SpriteBatch sb, SpritePrimitiveRenderer prim, IReadOnlyList<Vector2> v,
        Color color)
    {
        for (var i = 0; i < v.Count; i++)
            prim.DrawLine(sb, v[i], v[(i + 1) % v.Count], color, thickness: 2);
    }
}

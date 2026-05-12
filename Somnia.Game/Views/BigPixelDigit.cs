using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Somnia.Game.Views;

/// <summary>
/// Битовый «семисегментный»-style шрифт цифр 0..9, ':' и ' '.
/// Каждая цифра рисуется матрицей 5×9 точек (включая пустые строки сверху/снизу).
/// Можно рисовать гигантским размером — pixelSize задаёт сторону одной точки.
/// </summary>
public static class BigPixelDigit
{
    public const int GlyphCols = 5;
    public const int GlyphRows = 9;

    /// <summary>Битовая маска символа: каждый bit высот×ширины задаёт «1» — есть пиксель.</summary>
    private static readonly System.Collections.Generic.Dictionary<char, string[]> Glyphs = new()
    {
        ['0'] = new[]
        {
            "01110",
            "10001",
            "10011",
            "10011",
            "10101",
            "11001",
            "11001",
            "10001",
            "01110",
        },
        ['1'] = new[]
        {
            "00100",
            "01100",
            "10100",
            "00100",
            "00100",
            "00100",
            "00100",
            "00100",
            "11111",
        },
        ['2'] = new[]
        {
            "01110",
            "10001",
            "00001",
            "00010",
            "00100",
            "01000",
            "10000",
            "10000",
            "11111",
        },
        ['3'] = new[]
        {
            "11110",
            "00001",
            "00001",
            "00001",
            "01110",
            "00001",
            "00001",
            "00001",
            "11110",
        },
        ['4'] = new[]
        {
            "00010",
            "00110",
            "01010",
            "10010",
            "11111",
            "00010",
            "00010",
            "00010",
            "00010",
        },
        ['5'] = new[]
        {
            "11111",
            "10000",
            "10000",
            "11110",
            "00001",
            "00001",
            "00001",
            "10001",
            "01110",
        },
        ['6'] = new[]
        {
            "00110",
            "01000",
            "10000",
            "11110",
            "10001",
            "10001",
            "10001",
            "10001",
            "01110",
        },
        ['7'] = new[]
        {
            "11111",
            "00001",
            "00010",
            "00010",
            "00100",
            "00100",
            "01000",
            "01000",
            "10000",
        },
        ['8'] = new[]
        {
            "01110",
            "10001",
            "10001",
            "10001",
            "01110",
            "10001",
            "10001",
            "10001",
            "01110",
        },
        ['9'] = new[]
        {
            "01110",
            "10001",
            "10001",
            "10001",
            "01111",
            "00001",
            "00001",
            "00010",
            "01100",
        },
        [':'] = new[]
        {
            "00000",
            "00000",
            "01100",
            "01100",
            "00000",
            "00000",
            "01100",
            "01100",
            "00000",
        },
        [' '] = new[]
        {
            "00000",
            "00000",
            "00000",
            "00000",
            "00000",
            "00000",
            "00000",
            "00000",
            "00000",
        }
    };

    public static int MeasureWidth(string text, int pixelSize, int pixelSpacing = 1, int glyphSpacing = 2)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length * (GlyphCols * pixelSize + (GlyphCols - 1) * pixelSpacing + glyphSpacing * pixelSize);
    }

    public static int MeasureHeight(int pixelSize, int pixelSpacing = 1)
        => GlyphRows * pixelSize + (GlyphRows - 1) * pixelSpacing;

    /// <summary>Рисует текст в верхнем левом углу позиции <paramref name="topLeft"/> битмапным шрифтом.</summary>
    public static void Draw(SpriteBatch sb, Texture2D pixel, string text, Vector2 topLeft,
        Color color, int pixelSize, int pixelSpacing = 1, int glyphSpacing = 2)
    {
        var x0 = topLeft.X;
        foreach (var ch in text)
        {
            if (!Glyphs.TryGetValue(ch, out var rows)) continue;
            DrawGlyph(sb, pixel, rows, x0, topLeft.Y, color, pixelSize, pixelSpacing);
            x0 += GlyphCols * pixelSize + (GlyphCols - 1) * pixelSpacing + glyphSpacing * pixelSize;
        }
    }

    private static void DrawGlyph(SpriteBatch sb, Texture2D pixel, string[] rows, float x0, float y0,
        Color color, int pixelSize, int pixelSpacing)
    {
        for (var r = 0; r < GlyphRows; r++)
        {
            var row = rows[r];
            for (var c = 0; c < GlyphCols; c++)
            {
                if (row[c] != '1') continue;
                var x = (int)(x0 + c * (pixelSize + pixelSpacing));
                var y = (int)(y0 + r * (pixelSize + pixelSpacing));
                sb.Draw(pixel, new Rectangle(x, y, pixelSize, pixelSize), color);
            }
        }
    }
}

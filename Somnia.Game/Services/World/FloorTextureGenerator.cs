using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>
/// Генерация текстуры пола (изолинии по фрактальному шуму Перлина): линии и фон — оттенки серого, настраиваются в <see cref="FloorTextureSettings"/>.
/// Один раз на арену: текстура тайлится по экрану — быстрее и проще, чем полноэкранный шейдер каждый кадр.
/// </summary>
public static class FloorTextureGenerator
{
    /// <summary>
    /// Строит новую текстуру пола для уровня. Вызывать при смене арены / нового seed.
    /// Старый экземпляр <see cref="Texture2D"/> нужно <see cref="GraphicsResource.Dispose"/> перед заменой.
    /// </summary>
    public static Texture2D? GenerateLevel(GraphicsDevice device, FloorTextureSettings settings, int levelSeed)
    {
        if (!settings.UseProceduralFloor) return null;

        var size = Math.Clamp(settings.TextureSize, 64, 2048);
        // степень двойки — меньше артефактов при mip и фильтрации
        size = NextPow2(size);

        var noise = new PerlinNoise2D(levelSeed);
        var data = new Color[size * size];

        var bands = Math.Max(4, settings.ContourBands);
        var thick = Math.Clamp(settings.LineThickness, 0.005f, 0.45f);
        var scale = Math.Max(0.05f, settings.Scale);
        var oct = Math.Clamp(settings.Octaves, 1, 12);
        var pers = Math.Clamp(settings.Persistence, 0.01f, 1f);
        var lac = Math.Max(1.01f, settings.Lacunarity);
        var lineB = Math.Clamp(settings.LineBrightness, 0f, 1f);
        var bgB = Math.Clamp(settings.BackgroundBrightness, 0f, 1f);
        var lineByte = (byte)Math.Round(lineB * 255f);
        var bgByte = (byte)Math.Round(bgB * 255f);
        var lineColor = new Color(lineByte, lineByte, lineByte);
        var bgColor = new Color(bgByte, bgByte, bgByte);

        var inv = 1f / size;
        for (var py = 0; py < size; py++)
        for (var px = 0; px < size; px++)
        {
            var nx = (px + 0.5f) * inv * scale;
            var ny = (py + 0.5f) * inv * scale;

            var raw = noise.FractalBrownian(nx, ny, oct, pers, lac);
            var h = raw * 0.5f + 0.5f;
            if (h < 0f) h = 0f;
            else if (h > 1f) h = 1f;

            var elevated = h * bands;
            var frac = elevated - MathF.Floor(elevated);

            var onLine = frac < thick || frac > 1f - thick;
            var idx = py * size + px;
            data[idx] = onLine ? lineColor : bgColor;
        }

        var tex = new Texture2D(device, size, size, false, SurfaceFormat.Color);
        tex.SetData(data);
        tex.Name = $"floor_proc_seed_{levelSeed}";
        return tex;
    }

    private static int NextPow2(int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        return Math.Clamp(v + 1, 64, 2048);
    }
}

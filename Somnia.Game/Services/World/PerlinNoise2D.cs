using System;

namespace Somnia.Game.Services.World;

/// <summary>Детерминированный 2D-шум Перлина с перестановочной таблицей от seed.</summary>
public sealed class PerlinNoise2D
{
    private readonly int[] _perm = new int[512];

    public PerlinNoise2D(int seed)
    {
        var rnd = new Random(seed);
        Span<int> order = stackalloc int[256];
        for (var i = 0; i < 256; i++) order[i] = i;

        for (var i = 255; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        for (var i = 0; i < 256; i++)
        {
            _perm[i] = order[i];
            _perm[i + 256] = order[i];
        }
    }

    /// <summary>Значение примерно в диапазоне [-1, 1].</summary>
    public float Noise(float x, float y)
    {
        var xi = (int)MathF.Floor(x) & 255;
        var yi = (int)MathF.Floor(y) & 255;
        var xf = x - MathF.Floor(x);
        var yf = y - MathF.Floor(y);

        var u = Fade(xf);
        var v = Fade(yf);

        var aa = _perm[_perm[xi] + yi];
        var ab = _perm[_perm[xi] + yi + 1];
        var ba = _perm[_perm[xi + 1] + yi];
        var bb = _perm[_perm[xi + 1] + yi + 1];

        var x1 = Lerp(u, Grad(aa, xf, yf), Grad(ba, xf - 1, yf));
        var x2 = Lerp(u, Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1));
        return Lerp(v, x1, x2);
    }

    /// <summary>Фрактальный суммарный шум (FBM), нормализованный примерно к [-1, 1].</summary>
    public float FractalBrownian(float x, float y, int octaves, float persistence, float lacunarity)
    {
        octaves = Math.Clamp(octaves, 1, 12);
        var total = 0f;
        var amplitude = 1f;
        var frequency = 1f;
        var norm = 0f;

        for (var o = 0; o < octaves; o++)
        {
            total += Noise(x * frequency, y * frequency) * amplitude;
            norm += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return norm > 1e-6f ? total / norm : 0f;
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    private static float Lerp(float t, float a, float b) => a + t * (b - a);

    private static float Grad(int hash, float x, float y)
    {
        var h = hash & 7;
        var u = h < 4 ? x : y;
        var v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}

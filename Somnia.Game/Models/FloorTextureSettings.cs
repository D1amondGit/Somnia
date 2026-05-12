using System;

namespace Somnia.Game.Models;

/// <summary>
/// Параметры процедурной текстуры пола (изолинии по шуму Перлина).
/// Меняй значения здесь или в отладчике на живом экземпляре в <see cref="Session.GameplaySessionState.FloorTexture"/>.
/// </summary>
public sealed class FloorTextureSettings
{
    /// <summary>Масштаб сэмплирования шума по UV (больше — мельче «рельеф», меньше — крупнее формы).</summary>
    public float Scale { get; set; } = 2f;

    /// <summary>
    /// Относительная толщина линии в долях одного «ступенчатого» интервала после умножения на <see cref="ContourBands"/>.
    /// Диапазон обычно 0.02–0.15: меньше — тоньше линии, больше — жирнее.
    /// </summary>
    public float LineThickness { get; set; } = 0.03f;

    /// <summary>
    /// Яркость линий изолиний: 0 = чёрные (сливаются с фоном), 1 = белые.
    /// Для серых линий как у топографии обычно 0.2–0.55 (по умолчанию средний серый).
    /// Редактируй в коде или в отладчике: <c>_session.FloorTexture.LineBrightness</c>.
    /// </summary>
    public float LineBrightness { get; set; } = 0.25f;

    /// <summary>
    /// Яркость фона между линиями: 0 = чёрный, чуть выше — тёмно-серый «туман».
    /// </summary>
    public float BackgroundBrightness { get; set; } = 0.01f;

    /// <summary>Сколько слоёв шума суммируется (больше — богаче мелкие детали, дороже генерация).</summary>
    public int Octaves { get; set; } = 5;

    /// <summary>Вклад каждого следующего октава (0–1): ниже — более гладкий «крупный» рельеф.</summary>
    public float Persistence { get; set; } = 0.5f;

    /// <summary>Множитель частоты между октавами: выше — мельче детали на каждом шаге.</summary>
    public float Lacunarity { get; set; } = 2.5f;

    /// <summary>
    /// Плотность изолиний: во сколько условных «уровней» режется диапазон 0…1 после FBM.
    /// Больше — чаще линии, меньше — реже.
    /// </summary>
    public int ContourBands { get; set; } = 13;

    /// <summary>Размер стороны квадратной текстуры (степень двойки для GPU комфорта: 256 / 512).</summary>
    public int TextureSize { get; set; } = 512;

    /// <summary>При false используется только Content/floor.png при наличии.</summary>
    public bool UseProceduralFloor { get; set; } = true;

    /// <summary>Сводка параметров для пересборки текстуры при изменении настроек без смены арены.</summary>
    public int ComputeFingerprint()
    {
        var h = new HashCode();
        h.Add(Scale);
        h.Add(LineThickness);
        h.Add(LineBrightness);
        h.Add(BackgroundBrightness);
        h.Add(Octaves);
        h.Add(Persistence);
        h.Add(Lacunarity);
        h.Add(ContourBands);
        h.Add(TextureSize);
        h.Add(UseProceduralFloor);
        return h.ToHashCode();
    }
}

namespace Somnia.Game.Models;

/// <summary>
/// Единый источник правды для изометрической перспективы мира.
/// <para>Squash сжимает Y относительно X (меньше — площе).</para>
/// <para>Tilt сдвигает верхние точки фигуры по X пропорционально X (больше — сильнее «завал»).</para>
/// Меняй здесь — поменяется визуал и физика во всей игре одновременно.
/// </summary>
public static class IsometricView
{
    public const float Squash = 0.55f;
    public const float Tilt = 0.09f;
}

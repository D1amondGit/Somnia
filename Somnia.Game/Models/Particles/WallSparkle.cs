using Microsoft.Xna.Framework;

namespace Somnia.Game.Models.Particles;

/// <summary>
/// «Искра» на верхушке/грани стены — короткая блестящая точка.
/// Эмитируются периодически по всем стенам арены.
/// </summary>
public sealed class WallSparkle
{
    public Vector2 Position;
    public float Lifetime;
    public float MaxLifetime;
    public float Size;
    public Color Color;

    public float Alpha => MathHelper.Clamp(Lifetime / MaxLifetime, 0f, 1f);
}

using Microsoft.Xna.Framework;

namespace Somnia.Game.Models.Particles;

/// <summary>
/// «Брызги» на полу от попадания. Несколько коротко-живущих точек разной формы.
/// </summary>
public sealed class FloorSplatter
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius;
    public float Lifetime;
    public float MaxLifetime;
    public Color Color;
    public bool IsScorch;

    public float Alpha => MathHelper.Clamp(Lifetime / MaxLifetime, 0f, 1f);
}

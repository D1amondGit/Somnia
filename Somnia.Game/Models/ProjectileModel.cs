using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>Снаряд врага (стрелок).</summary>
public class ProjectileModel
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius;
    public float LifeTime = 3f;

    public ProjectileModel(Vector2 p, Vector2 v, float r)
    {
        Position = p;
        Velocity = v;
        Radius = r;
    }
}

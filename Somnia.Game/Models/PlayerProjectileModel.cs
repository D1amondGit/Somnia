using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>Быстрый снаряд игрока с отложенным нанесением урона при попадании.</summary>
public class PlayerProjectileModel
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 6f;
    public float LifeRemaining = 3f;
    public float Damage;
    public float Knockback;
    public Vector2 DamageSource;
    /// <summary>Макс. пройденная дистанция (0 = без лимита).</summary>
    public float MaxTravelDistance;
    public float DistanceTraveled;
    public PlayerProjectileKind Kind;

    /// <summary>Урон по области при Kind == RocketImpact.</summary>
    public float ExplosionRadius;

    /// <summary>Уже нанесли взрыв (ракета).</summary>
    public bool Exploded;
}

public enum PlayerProjectileKind
{
    Pellet,
    Bolt,
    Rocket
}

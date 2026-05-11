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

    /// <summary>Радиус AoE для ракеты/гранаты.</summary>
    public float ExplosionRadius;

    /// <summary>Уже сработал взрыв/детонация.</summary>
    public bool Exploded;

    /// <summary>HP, восстанавливаемые игроку и NPC при разрыве гранаты.</summary>
    public float HealAmount;

    /// <summary>Длительность яда (инфекции) на врагов при разрыве гранаты.</summary>
    public float PoisonDuration;
}

public enum PlayerProjectileKind
{
    Pellet,
    Bolt,
    Rocket,
    Grenade
}

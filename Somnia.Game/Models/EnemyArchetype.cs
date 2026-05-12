using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>
/// Конфиг класса врага: всё, что зависит от типа — здесь.
/// Никаких <c>if (type == Foo)</c> внутри AI/View; они читают свойства архетипа.
/// </summary>
public sealed record EnemyArchetype(
    EnemyType Type,
    float MaxHealth,
    float MoveSpeed,
    float MeleeDamage,
    float MeleeReach,
    float AttackCooldown,
    float ProjectileSpeed,
    float ProjectileDamage,
    float ProjectileRadius,
    float TelegraphTime,
    float EngageRange,
    float PreferredRange,
    float BodyRadius,
    float BodyHeight,
    Color BodyColor,
    Color AccentColor,
    bool IgnoresSteering,
    bool ExplodesOnContact);

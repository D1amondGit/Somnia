using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>Единый каталог архетипов. Тут балансим параметры классов.</summary>
public static class EnemyArchetypeCatalog
{
    private static readonly Dictionary<EnemyType, EnemyArchetype> Map = new()
    {
        [EnemyType.Melee] = new EnemyArchetype(
            Type: EnemyType.Melee,
            MaxHealth: 75f,
            MoveSpeed: 230f,
            MeleeDamage: 14f,
            MeleeReach: 70f,
            AttackCooldown: 0.75f,
            ProjectileSpeed: 0f,
            ProjectileDamage: 0f,
            ProjectileRadius: 0f,
            TelegraphTime: 0f,
            EngageRange: 900f,
            PreferredRange: 0f,
            BodyRadius: 22f,
            BodyHeight: 36f,
            BodyColor: new Color(160, 50, 180),
            AccentColor: new Color(220, 110, 240),
            IgnoresSteering: false,
            ExplodesOnContact: false),

        [EnemyType.Shooter] = new EnemyArchetype(
            Type: EnemyType.Shooter,
            MaxHealth: 55f,
            MoveSpeed: 160f,
            MeleeDamage: 0f,
            MeleeReach: 0f,
            AttackCooldown: 1.5f,
            ProjectileSpeed: 580f,
            ProjectileDamage: 12f,
            ProjectileRadius: 10f,
            TelegraphTime: 0f,
            EngageRange: 900f,
            PreferredRange: 420f,
            BodyRadius: 20f,
            BodyHeight: 42f,
            BodyColor: new Color(60, 90, 200),
            AccentColor: new Color(140, 180, 255),
            IgnoresSteering: false,
            ExplodesOnContact: false),

        [EnemyType.Charger] = new EnemyArchetype(
            Type: EnemyType.Charger,
            MaxHealth: 35f,
            MoveSpeed: 360f,
            MeleeDamage: 30f,
            MeleeReach: 55f,
            AttackCooldown: 0.4f,
            ProjectileSpeed: 0f,
            ProjectileDamage: 0f,
            ProjectileRadius: 0f,
            TelegraphTime: 0f,
            EngageRange: 1300f,
            PreferredRange: 0f,
            BodyRadius: 18f,
            BodyHeight: 28f,
            BodyColor: new Color(230, 90, 60),
            AccentColor: new Color(255, 180, 120),
            IgnoresSteering: true,
            ExplodesOnContact: true),

        [EnemyType.Sniper] = new EnemyArchetype(
            Type: EnemyType.Sniper,
            MaxHealth: 50f,
            MoveSpeed: 130f,
            MeleeDamage: 0f,
            MeleeReach: 0f,
            AttackCooldown: 2.2f,
            ProjectileSpeed: 1200f,
            ProjectileDamage: 28f,
            ProjectileRadius: 7f,
            TelegraphTime: 0.55f,
            EngageRange: 1500f,
            PreferredRange: 720f,
            BodyRadius: 19f,
            BodyHeight: 50f,
            BodyColor: new Color(190, 60, 130),
            AccentColor: new Color(255, 150, 200),
            IgnoresSteering: false,
            ExplodesOnContact: false),

        // Босс: толстый, медленный, особый AI (см. BossController).
        // MoveSpeed используется только во время Charge-фазы (через множитель).
        [EnemyType.Boss] = new EnemyArchetype(
            Type: EnemyType.Boss,
            MaxHealth: 900f,
            MoveSpeed: 110f,
            MeleeDamage: 24f,
            MeleeReach: 90f,
            AttackCooldown: 1.6f,
            ProjectileSpeed: 540f,
            ProjectileDamage: 14f,
            ProjectileRadius: 12f,
            TelegraphTime: 1.4f,
            EngageRange: 2400f,
            PreferredRange: 360f,
            BodyRadius: 48f,
            BodyHeight: 78f,
            BodyColor: new Color(120, 30, 60),
            AccentColor: new Color(255, 90, 130),
            IgnoresSteering: true,
            ExplodesOnContact: false)
    };

    public static EnemyArchetype Get(EnemyType type) =>
        Map.TryGetValue(type, out var a) ? a : Map[EnemyType.Melee];
}

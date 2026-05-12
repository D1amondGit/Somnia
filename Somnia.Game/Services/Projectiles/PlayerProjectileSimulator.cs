using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.Particles;
using Somnia.Game.Services.World;

namespace Somnia.Game.Services.Projectiles;

/// <summary>Симуляция быстрых снарядов игрока: полёт, взрывы ракет, разрыв гранат (хил + яд).</summary>
public sealed class PlayerProjectileSimulator
{
    private readonly ILineOfSightService _los;

    private const float EnemyHitRadiusApprox = 22f;

    private int _explosionsThisFrame;
    private int _directHitsThisFrame;
    private int _enemiesDamagedByAoeThisFrame;

    public PlayerProjectileSimulator(ILineOfSightService los) => _los = los;

    public int ConsumeExplosionEvents()
    {
        var e = _explosionsThisFrame;
        _explosionsThisFrame = 0;
        return e;
    }

    /// <summary>Сколько разных врагов получило прямое попадание (не AoE) в последнем кадре.</summary>
    public int ConsumeDirectHits()
    {
        var h = _directHitsThisFrame;
        _directHitsThisFrame = 0;
        return h;
    }

    /// <summary>Сколько врагов задело AoE-эффектом (ракета/граната).</summary>
    public int ConsumeAoeHits()
    {
        var a = _enemiesDamagedByAoeThisFrame;
        _enemiesDamagedByAoeThisFrame = 0;
        return a;
    }

    /// <summary>Список разрушаемых стен, в которые попал AoE в этом кадре (для удаления орестратором).</summary>
    public readonly List<HexagonModel> BrokenWallsThisFrame = new();

    public void Update(float dt, List<PlayerProjectileModel> projectiles, List<EnemyModel> enemies,
        List<HexagonModel> walls, PlayerModel? player = null, NpcModel? npc = null,
        FloorEffectService? fx = null, List<FloorSplatter>? splatters = null)
    {
        BrokenWallsThisFrame.Clear();
        _explosionsThisFrame = 0;
        _directHitsThisFrame = 0;
        _enemiesDamagedByAoeThisFrame = 0;

        for (var i = projectiles.Count - 1; i >= 0; i--)
        {
            var pr = projectiles[i];
            if (pr.LifeRemaining <= 0f || pr.Exploded)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            var displacement = pr.Velocity * dt;
            var nextPos = pr.Position + displacement;
            var moved = displacement.LengthSquared() > 0.0001f;

            if (moved && !_los.HasLineOfSight(pr.Position, nextPos, walls))
            {
                DetonateAoe(pr, enemies, player, npc, pr.Position, walls, fx, splatters);
                // Прямое попадание в стену тоже её точит — небольшой урон по разрушаемой.
                DamageDestructibleWallsAt(walls, pr.Position, pr.Radius + 18f, pr.Damage * 0.5f);
                if (fx != null && splatters != null)
                    fx.EmitImpact(splatters, pr.Position, ImpactColor(pr.Kind), 6, 110f);
                projectiles.RemoveAt(i);
                continue;
            }

            var distStep = displacement.Length();
            pr.DistanceTraveled += distStep;
            pr.Position = nextPos;
            pr.LifeRemaining -= dt;

            if (pr.LifeRemaining <= 0f && IsAoeKind(pr.Kind) && !pr.Exploded)
            {
                DetonateAoe(pr, enemies, player, npc, pr.Position, walls, fx, splatters);
                projectiles.RemoveAt(i);
                continue;
            }

            if (IsAoeKind(pr.Kind) && !pr.Exploded &&
                pr.MaxTravelDistance > 0f &&
                pr.DistanceTraveled >= pr.MaxTravelDistance)
            {
                DetonateAoe(pr, enemies, player, npc, pr.Position, walls, fx, splatters);
                projectiles.RemoveAt(i);
                continue;
            }

            if (!IsAoeKind(pr.Kind) &&
                pr.MaxTravelDistance > 0f &&
                pr.DistanceTraveled >= pr.MaxTravelDistance)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            var hitHull = ClosestEnemyInRange(pr.Position, enemies, pr.Radius + EnemyHitRadiusApprox);
            if (hitHull != null)
            {
                if (IsAoeKind(pr.Kind))
                {
                    DetonateAoe(pr, enemies, player, npc, pr.Position, walls, fx, splatters);
                }
                else
                {
                    hitHull.TakeDamage(pr.Damage, pr.DamageSource, pr.Knockback);
                    _directHitsThisFrame++;
                    if (fx != null && splatters != null)
                        fx.EmitImpact(splatters, hitHull.Position, BloodColor(), 12, 170f);
                }

                projectiles.RemoveAt(i);
                continue;
            }

            if (pr.LifeRemaining <= 0f)
                projectiles.RemoveAt(i);
        }
    }

    private static bool IsAoeKind(PlayerProjectileKind k) =>
        k == PlayerProjectileKind.Rocket || k == PlayerProjectileKind.Grenade;

    private static EnemyModel? ClosestEnemyInRange(Vector2 pt, List<EnemyModel> enemies, float radius)
    {
        var rsq = radius * radius;
        EnemyModel? best = null;
        var bd = rsq;

        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            var d = Vector2.DistanceSquared(pt, e.Position);
            if (!(d <= rsq)) continue;
            if (best == null || d < bd)
            {
                bd = d;
                best = e;
            }
        }

        return best;
    }

    private void DetonateAoe(PlayerProjectileModel pr, List<EnemyModel> enemies,
        PlayerModel? player, NpcModel? npc, Vector2 center,
        List<HexagonModel> walls,
        FloorEffectService? fx, List<FloorSplatter>? splatters)
    {
        if (pr.Exploded) return;
        pr.Exploded = true;
        _explosionsThisFrame++;

        DamageDestructibleWallsAt(walls, center, pr.ExplosionRadius, pr.Damage);

        var rSq = pr.ExplosionRadius * pr.ExplosionRadius;

        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            if (Vector2.DistanceSquared(e.Position, center) > rSq) continue;

            if (pr.Damage > 0)
            {
                e.TakeDamage(pr.Damage, pr.DamageSource, pr.Knockback);
                _enemiesDamagedByAoeThisFrame++;
            }

            if (pr.PoisonDuration > 0)
            {
                e.IsInfected = true;
                e.InfectionTimer = pr.PoisonDuration;
            }
        }

        if (fx != null && splatters != null)
        {
            var scorch = pr.Kind == PlayerProjectileKind.Grenade
                ? new Color(70, 180, 120)
                : new Color(200, 90, 30);
            fx.EmitScorch(splatters, center, pr.ExplosionRadius * 0.7f, scorch, 22);
        }

        if (pr.HealAmount <= 0) return;

        if (player != null && Vector2.DistanceSquared(player.Position, center) <= rSq)
            player.Heal(pr.HealAmount);

        if (npc != null && !npc.IsDead && Vector2.DistanceSquared(npc.Position, center) <= rSq)
            npc.Health = MathHelper.Min(npc.MaxHealth, npc.Health + pr.HealAmount);
    }

    /// <summary>Наносит <paramref name="damage"/> всем разрушаемым стенам в радиусе <paramref name="radius"/>.</summary>
    private void DamageDestructibleWallsAt(List<HexagonModel> walls, Vector2 center, float radius, float damage)
    {
        if (damage <= 0f) return;
        var rSq = (radius + 30f) * (radius + 30f);
        foreach (var w in walls)
        {
            if (!w.IsDestructible || w.IsBroken) continue;
            if (Vector2.DistanceSquared(w.Center, center) > rSq) continue;
            w.DestructibleHealth -= damage;
            if (w.DestructibleHealth <= 0f && !BrokenWallsThisFrame.Contains(w))
                BrokenWallsThisFrame.Add(w);
        }
    }

    private static Color BloodColor() => new(220, 50, 60);

    private static Color ImpactColor(PlayerProjectileKind k) => k switch
    {
        PlayerProjectileKind.Pellet => new Color(220, 200, 120),
        PlayerProjectileKind.Bolt => new Color(170, 220, 255),
        PlayerProjectileKind.Rocket => new Color(255, 130, 70),
        PlayerProjectileKind.Grenade => new Color(130, 230, 130),
        _ => Color.White
    };
}

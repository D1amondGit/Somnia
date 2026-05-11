using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Game.Services.Projectiles;

/// <summary>Симуляция быстрых снарядов игрока: полёт до препятствия/цели, ракеты с AoE.</summary>
public sealed class PlayerProjectileSimulator
{
    private readonly ILineOfSightService _los;

    private const float EnemyHitRadiusApprox = 22f;

    public PlayerProjectileSimulator(ILineOfSightService los) => _los = los;

    public void Update(float dt, List<PlayerProjectileModel> projectiles, List<EnemyModel> enemies,
        List<HexagonModel> walls)
    {
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
                if (pr.Kind == PlayerProjectileKind.Rocket && !pr.Exploded)
                    RocketExplosion.Explode(pr, enemies, pr.Position);
                projectiles.RemoveAt(i);
                continue;
            }

            var distStep = displacement.Length();
            pr.DistanceTraveled += distStep;
            pr.Position = nextPos;
            pr.LifeRemaining -= dt;

            if (pr.Kind == PlayerProjectileKind.Rocket &&
                !pr.Exploded &&
                pr.MaxTravelDistance > 0f &&
                pr.DistanceTraveled >= pr.MaxTravelDistance)
            {
                RocketExplosion.Explode(pr, enemies, pr.Position);
                projectiles.RemoveAt(i);
                continue;
            }

            if (pr.Kind != PlayerProjectileKind.Rocket &&
                pr.MaxTravelDistance > 0f &&
                pr.DistanceTraveled >= pr.MaxTravelDistance)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            var hitHull = ClosestEnemyInRange(pr.Position, enemies, pr.Radius + EnemyHitRadiusApprox);
            if (hitHull != null)
            {
                if (pr.Kind == PlayerProjectileKind.Rocket && !pr.Exploded)
                    RocketExplosion.Explode(pr, enemies, pr.Position);
                else
                    hitHull.TakeDamage(pr.Damage, pr.DamageSource, pr.Knockback);

                projectiles.RemoveAt(i);
                continue;
            }

            if (pr.LifeRemaining <= 0f)
                projectiles.RemoveAt(i);
        }
    }

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

    /// <summary>Единое место взрыва ракеты, чтобы избежать двойного урона по точке столкновения.</summary>
    private static class RocketExplosion
    {
        public static void Explode(PlayerProjectileModel pr, List<EnemyModel> enemies, Vector2 center)
        {
            if (pr.Exploded) return;
            pr.Exploded = true;
            var rSq = pr.ExplosionRadius * pr.ExplosionRadius;

            foreach (var e in enemies)
            {
                if (e.IsDead) continue;
                var d = Vector2.DistanceSquared(e.Position, center);
                if (d > rSq) continue;

                e.TakeDamage(pr.Damage, pr.DamageSource, pr.Knockback);
            }
        }
    }
}

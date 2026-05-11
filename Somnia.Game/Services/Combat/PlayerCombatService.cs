using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Services.World;

namespace Somnia.Game.Services.Combat;

public interface IPlayerCombatService
{
    /// <summary>Пытается выполнить навык по направлению курсора. Возвращает true при успехе.</summary>
    bool TryUseActiveSkill(PlayerModel player, Vector2 aimWorld,
        List<EnemyModel> enemies, NpcModel npc, List<HexagonModel> walls,
        List<PlayerProjectileModel> spawnProjectiles);
}

public sealed class PlayerCombatService : IPlayerCombatService
{
    private readonly ILineOfSightService _los;

    private const float PelletSpeed = 1150f;
    private const float BoltSpeed = 1350f;
    private const float RocketSpeed = 950f;

    public PlayerCombatService(ILineOfSightService los) => _los = los;

    public bool TryUseActiveSkill(PlayerModel player, Vector2 aimWorld,
        List<EnemyModel> enemies, NpcModel npc, List<HexagonModel> walls,
        List<PlayerProjectileModel> spawnProjectiles)
    {
        if (player.State == PlayerState.Carrying) return false;
        if (player.SkillOnCooldown) return false;

        var aimDir = aimWorld - player.Position;
        if (aimDir == Vector2.Zero) aimDir = Vector2.UnitX;
        else aimDir.Normalize();

        var m = player.DamageMultiplier;
        var ok = player.CurrentZone switch
        {
            AnomalyType.Red => UseRed(player, aimDir, enemies, npc, m, walls, spawnProjectiles),
            AnomalyType.Blue => UseBlue(player, aimDir, enemies, m, walls, spawnProjectiles),
            AnomalyType.Green => UseGreen(player, aimDir, enemies, m, walls, spawnProjectiles),
            AnomalyType.Neutral => UseNeutral(player, aimDir, enemies, m, walls, spawnProjectiles),
            _ => false
        };

        if (ok) player.RegisterSkillExecuted();
        return ok;
    }

    private bool UseRed(PlayerModel p, Vector2 dir, List<EnemyModel> enemies, NpcModel npc, float m,
        List<HexagonModel> walls, List<PlayerProjectileModel> projs)
    {
        if (p.ActiveSlot == 0 && p.ConsumeMana(10f))
        {
            const int pelletCount = 6;
            var spread = 0.35f;
            for (var i = 0; i < pelletCount; i++)
            {
                var t = pelletCount == 1 ? 0f : (i / (float)(pelletCount - 1)) * 2f - 1f;
                var shotDir = Rotate(dir, t * spread);

                projs.Add(new PlayerProjectileModel
                {
                    Position = p.Position + shotDir * 25f,
                    Velocity = shotDir * PelletSpeed,
                    Damage = 100f * m / pelletCount,
                    Knockback = 900f,
                    DamageSource = p.Position,
                    LifeRemaining = 1.2f,
                    Kind = PlayerProjectileKind.Pellet
                });
            }

            p.MaxCd1 = 0.5f;
            return true;
        }

        if (p.ActiveSlot == 1 && p.ConsumeMana(20f))
        {
            var t = GetClosestEntity(p, dir, enemies, npc, 800f, 0.4f, walls);
            if (t is EnemyModel em)
            {
                em.Velocity += Vector2.Normalize(p.Position - em.Position) * 1500f;
                em.TakeDamage(10f, p.Position, 0f);
            }
            else if (t is NpcModel nm) nm.Position = Vector2.Lerp(nm.Position, p.Position, 0.85f);

            p.MaxCd2 = 2f;
            return true;
        }

        if (p.ActiveSlot == 2)
        {
            if (!HasLosFromPlayer(p, p.Position + dir * 50f, walls)) return false;
            if (!p.ConsumeMana(50f)) return false;

            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + dir * 30f,
                Velocity = dir * RocketSpeed,
                Damage = 200f * m,
                Knockback = 200f,
                DamageSource = p.Position,
                LifeRemaining = 2.5f,
                MaxTravelDistance = 820f,
                ExplosionRadius = 90f,
                Kind = PlayerProjectileKind.Rocket
            });

            p.MaxCd3 = 5f;
            return true;
        }

        return false;
    }

    private bool UseGreen(PlayerModel p, Vector2 dir, List<EnemyModel> enemies, float m,
        List<HexagonModel> walls, List<PlayerProjectileModel> projs)
    {
        if (p.ActiveSlot == 0)
        {
            if (p.CurrentMana < 10f) return false;
            var t = GetClosestEntity(p, dir, enemies, null, 1000f, 0.8f, walls);
            if (t is not EnemyModel em) return false;
            if (!p.ConsumeMana(10f)) return false;

            var shotDir = em.Position - p.Position;
            if (shotDir == Vector2.Zero) return false;
            shotDir.Normalize();

            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + shotDir * 22f,
                Velocity = shotDir * BoltSpeed,
                Damage = 40f * m,
                Knockback = 200f,
                DamageSource = p.Position,
                LifeRemaining = 1.5f,
                Kind = PlayerProjectileKind.Bolt
            });

            p.MaxCd1 = 0.8f;
            return true;
        }

        if (p.ActiveSlot == 1 && p.ConsumeMana(30f))
        {
            p.BeginGreenAura(4f);
            p.MaxCd2 = 5f;
            return true;
        }

        if (p.ActiveSlot == 2 && p.ConsumeMana(40f))
        {
            var t = GetClosestEntity(p, dir, enemies, null, 1000f, 0.8f, walls);
            if (t is EnemyModel em)
            {
                em.IsInfected = true;
                em.InfectionTimer = 0.1f;
            }

            p.MaxCd3 = 4f;
            return true;
        }

        return false;
    }

    private bool UseBlue(PlayerModel p, Vector2 dir, List<EnemyModel> enemies, float m,
        List<HexagonModel> walls, List<PlayerProjectileModel> projs)
    {
        if (p.ActiveSlot == 0 && p.ConsumeMana(5f))
        {
            if (!HasLosFromPlayer(p, p.Position + dir * 100f, walls)) return false;

            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + dir * 18f,
                Velocity = dir * BoltSpeed,
                Damage = 15f * m,
                Knockback = 100f,
                DamageSource = p.Position,
                LifeRemaining = 0.35f,
                MaxTravelDistance = 170f,
                Kind = PlayerProjectileKind.Bolt
            });

            p.MaxCd1 = 0.2f;
            return true;
        }

        if (p.ActiveSlot == 1 && p.ConsumeMana(25f))
        {
            p.ActivateSkillForcedDash();
            foreach (var e in enemies)
            {
                if (e.IsDead || Vector2.Distance(p.Position, e.Position) >= 250f) continue;
                if (!_los.HasLineOfSight(p.Position, e.Position, walls)) continue;
                e.StunTimer = 2.5f;
            }

            p.MaxCd2 = 3f;
            return true;
        }

        if (p.ActiveSlot == 2 && p.ConsumeMana(50f))
        {
            foreach (var e in enemies) e.SlowTimer = 4f;
            p.MaxCd3 = 10f;
            return true;
        }

        return false;
    }

    private bool UseNeutral(PlayerModel p, Vector2 dir, List<EnemyModel> enemies, float m,
        List<HexagonModel> walls, List<PlayerProjectileModel> projs)
    {
        if (p.ActiveSlot == 0 && p.ConsumeMana(5f))
        {
            if (!HasLosFromPlayer(p, p.Position + dir * 80f, walls)) return false;

            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + dir * 16f,
                Velocity = dir * BoltSpeed,
                Damage = 25f * m,
                Knockback = 300f,
                DamageSource = p.Position,
                LifeRemaining = 0.65f,
                MaxTravelDistance = 620f,
                Kind = PlayerProjectileKind.Bolt
            });

            p.MaxCd1 = 0.3f;
            return true;
        }

        return false;
    }

    private bool HasLosFromPlayer(PlayerModel p, Vector2 toPoint, List<HexagonModel> walls) =>
        _los.HasLineOfSight(p.Position, toPoint, walls);

    private object? GetClosestEntity(PlayerModel p, Vector2 aimDir, List<EnemyModel> enemies, NpcModel? npc,
        float maxRange, float minDot, List<HexagonModel> walls)
    {
        object? best = null;
        var bestDot = minDot;

        foreach (var e in enemies.Where(x => !x.IsDead))
        {
            var toE = e.Position - p.Position;
            var len = toE.Length();
            if (len < 1e-3f || len > maxRange) continue;

            var norm = toE / len;
            if (!_los.HasLineOfSight(p.Position, e.Position, walls)) continue;

            var dot = Vector2.Dot(aimDir, norm);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = e;
            }
        }

        if (npc is { IsPickedUp: false })
        {
            var toN = npc.Position - p.Position;
            var lenN = toN.Length();
            if (lenN > 1e-3f && lenN < maxRange)
            {
                var normN = toN / lenN;
                if (_los.HasLineOfSight(p.Position, npc.Position, walls))
                {
                    var dot = Vector2.Dot(aimDir, normN);
                    if (dot > bestDot)
                        best = npc;
                }
            }
        }

        return best;
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        var c = MathF.Cos(radians);
        var s = MathF.Sin(radians);
        return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
    }
}

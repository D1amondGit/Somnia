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

    /// <summary>Возвращает и сбрасывает накопленный «recoil trauma» — тряску камеры
    /// от выстрелов с отдачей (дробовик, снайперка, лёгкая от автомата).</summary>
    float ConsumeRecoilShake();
}

public sealed class PlayerCombatService : IPlayerCombatService
{
    private readonly ILineOfSightService _los;

    private const float PelletSpeed = 1300f;
    private const float BoltSpeed = 1500f;
    private const float SniperSpeed = 2400f;
    private const float RocketSpeed = 950f;
    private const float GrenadeSpeed = 700f;

    private float _recoilShakeAccum;

    public PlayerCombatService(ILineOfSightService los) => _los = los;

    public float ConsumeRecoilShake()
    {
        var v = _recoilShakeAccum;
        _recoilShakeAccum = 0f;
        return v;
    }

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
        // Слот 0 — ДРОБОВИК: короткая дистанция, большой урон, отдача, большой КД.
        if (p.ActiveSlot == 0 && p.ConsumeMana(15f))
        {
            const int pelletCount = 7;
            const float spread = 0.45f;
            for (var i = 0; i < pelletCount; i++)
            {
                var t = (i / (float)(pelletCount - 1)) * 2f - 1f;
                var shotDir = Rotate(dir, t * spread);

                projs.Add(new PlayerProjectileModel
                {
                    Position = p.Position + shotDir * 25f,
                    Velocity = shotDir * PelletSpeed,
                    Damage = 35f * m,
                    Knockback = 950f,
                    DamageSource = p.Position,
                    LifeRemaining = 0.25f,
                    MaxTravelDistance = 320f,
                    Kind = PlayerProjectileKind.Pellet
                });
            }

            p.ApplyKnockback(-dir * 380f);
            _recoilShakeAccum += 0.32f;
            p.MaxCd1 = 1.1f;
            return true;
        }

        if (p.ActiveSlot == 1 && p.ConsumeMana(20f))
        {
            var t = GetClosestEntity(p, dir, enemies, npc, 800f, 0.4f, walls);
            if (t is EnemyModel em)
            {
                var pull = p.Position - em.Position;
                if (pull.LengthSquared() > 1e-4f)
                    em.Velocity += Vector2.Normalize(pull) * 1500f;
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
        // Слот 0 — ГРАНАТА: AoE, хилит игрока и NPC, отравляет врагов.
        if (p.ActiveSlot == 0 && p.ConsumeMana(25f))
        {
            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + dir * 28f,
                Velocity = dir * GrenadeSpeed,
                Damage = 30f * m,
                Knockback = 0f,
                DamageSource = p.Position,
                LifeRemaining = 1.8f,
                MaxTravelDistance = 520f,
                ExplosionRadius = 260f,
                HealAmount = 45f,
                PoisonDuration = 1.8f,
                Kind = PlayerProjectileKind.Grenade
            });

            p.MaxCd1 = 1.6f;
            return true;
        }

        // Слот 1 — ЩИТ: входящий урон режется на 65%, всех врагов в радиусе резко выталкивает.
        if (p.ActiveSlot == 1 && p.ConsumeMana(30f))
        {
            p.BeginShield(durationSeconds: 3.5f, radius: 240f, reduction: 0.65f);

            // Активирующий импульс — отталкивающий «толчок» по всему радиусу.
            foreach (var e in enemies)
            {
                if (e.IsDead) continue;
                var diff = e.Position - p.Position;
                var d = diff.Length();
                if (d <= 0f || d > 240f) continue;
                e.Velocity += Vector2.Normalize(diff) * (1200f * (1f - d / 240f) + 200f);
            }

            p.MaxCd2 = 5f;
            return true;
        }

        // Слот 2 — ЗАРАЖЕНИЕ: основная цель + цепная передача через HandleInfection в AI.
        if (p.ActiveSlot == 2 && p.ConsumeMana(35f))
        {
            var t = GetClosestEntity(p, dir, enemies, null, 1100f, 0.65f, walls);
            if (t is EnemyModel em)
            {
                em.IsInfected = true;
                em.InfectionTimer = 0.6f;
                // Урон по самому заражаемому, чтобы было ощущение касания.
                em.TakeDamage(25f * m, p.Position, 200f);
            }

            p.MaxCd3 = 4f;
            return true;
        }

        return false;
    }

    private bool UseBlue(PlayerModel p, Vector2 dir, List<EnemyModel> enemies, float m,
        List<HexagonModel> walls, List<PlayerProjectileModel> projs)
    {
        // Слот 0 — СНАЙПЕРКА: через всю арену, огромный урон, средний КД.
        if (p.ActiveSlot == 0 && p.ConsumeMana(20f))
        {
            if (!HasLosFromPlayer(p, p.Position + dir * 80f, walls)) return false;

            projs.Add(new PlayerProjectileModel
            {
                Position = p.Position + dir * 22f,
                Velocity = dir * SniperSpeed,
                Damage = 180f * m,
                Knockback = 600f,
                DamageSource = p.Position,
                LifeRemaining = 1.2f,
                MaxTravelDistance = 2400f,
                Kind = PlayerProjectileKind.Bolt
            });

            _recoilShakeAccum += 0.18f;
            p.MaxCd1 = 1.5f;
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
        // Слот 0 — АВТОМАТ: средняя дистанция, очередь из 3 пуль, слабая отдача, малый КД.
        if (p.ActiveSlot == 0 && p.ConsumeMana(6f))
        {
            const int burst = 3;
            const float spread = 0.06f;
            for (var i = 0; i < burst; i++)
            {
                var t = (i / (float)(burst - 1)) * 2f - 1f;
                var shotDir = Rotate(dir, t * spread);

                projs.Add(new PlayerProjectileModel
                {
                    Position = p.Position + shotDir * (16f + i * 4f),
                    Velocity = shotDir * BoltSpeed,
                    Damage = 10f * m,
                    Knockback = 180f,
                    DamageSource = p.Position,
                    LifeRemaining = 0.85f,
                    MaxTravelDistance = 720f,
                    Kind = PlayerProjectileKind.Bolt
                });
            }

            p.ApplyKnockback(-dir * 60f);
            _recoilShakeAccum += 0.08f;
            p.MaxCd1 = 0.32f;
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

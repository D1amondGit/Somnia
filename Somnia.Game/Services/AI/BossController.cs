using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.AI;

/// <summary>Конечный автомат атак босса (<see cref="BossAttackPhase"/>).</summary>
public static class BossController
{
    public const float IdleDuration = 1.1f;
    public const float SlamTelegraphTime = 1.25f;
    public const float SlamRadius = 240f;
    public const float SlamDamagePlayer = 32f;
    public const float SlamDamageNpc = 22f;

    public const float VolleyTelegraphTime = 0.9f;
    public const int VolleyShots = 9;
    public const float VolleyFanAngle = MathHelper.Pi * 0.55f;

    public const float ChargeDuration = 1.4f;
    public const float ChargeSpeedMultiplier = 4.2f;
    public const float ChargeMeleeDamage = 28f;

    /// <summary>Дробит здоровье босса на «акты» — для подбора паттернов.</summary>
    public const float Phase2HpFraction = 0.65f; // подключает Volley
    public const float Phase3HpFraction = 0.30f; // подключает Charge + ускоряет паттерны

    /// <summary>Список разрушаемых стен, подломанных Slam'ом за этот кадр (для удаления оркестратором).</summary>
    public static readonly List<HexagonModel> BrokenWallsThisFrame = new();

    /// <summary>Мир-точка для прицеливания и преследования с учётом <see cref="EnemyModel.BossFocusOnNpc"/>.</summary>
    public static Vector2 GetStrikeFocusWorld(EnemyModel boss, PlayerModel player, NpcModel npc)
    {
        if (boss.BossFocusOnNpc && !npc.IsPickedUp && !npc.IsDead)
            return npc.Position;
        return player.Position;
    }

    public static void Update(
        EnemyModel boss,
        PlayerModel player,
        NpcModel npc,
        float distToStrikeFocus,
        float dt,
        List<ProjectileModel> projectiles,
        List<HexagonModel>? destructibleWalls = null)
    {
        boss.BossPhaseTimer -= dt;

        switch (boss.BossPhase)
        {
            case BossAttackPhase.Idle:
                TickIdle(boss, player, npc, distToStrikeFocus, dt);
                break;
            case BossAttackPhase.SlamTelegraph:
                TickSlamTelegraph(boss);
                break;
            case BossAttackPhase.SlamImpact:
                TickSlamImpact(boss, player, npc, destructibleWalls);
                break;
            case BossAttackPhase.VolleyTelegraph:
                TickVolleyTelegraph(boss, player, npc);
                break;
            case BossAttackPhase.VolleyFire:
                TickVolleyFire(boss, player, npc, projectiles);
                break;
            case BossAttackPhase.Charge:
                TickCharge(boss, player, npc, dt);
                break;
        }
    }

    private static void RefreshBossAggro(EnemyModel boss, PlayerModel player, NpcModel npc)
    {
        if (npc.IsPickedUp || npc.IsDead)
        {
            boss.BossFocusOnNpc = false;
            return;
        }

        var dNpc = Vector2.Distance(boss.Position, npc.Position);
        if (dNpc > 820f)
        {
            boss.BossFocusOnNpc = false;
            return;
        }

        boss.BossAggroRollNonce++;
        var seed = HashCode.Combine(boss.BossAggroRollNonce, (int)boss.Health, (int)boss.Position.X, (int)boss.Position.Y);
        var rng = new Random(seed);
        boss.BossFocusOnNpc = rng.NextDouble() < 0.34;
    }

    private static void TickIdle(EnemyModel boss, PlayerModel player, NpcModel npc, float dist, float dt)
    {
        var focus = GetStrikeFocusWorld(boss, player, npc);
        var dir = focus - boss.Position;
        if (dir != Vector2.Zero && dist > 220f)
        {
            dir.Normalize();
            boss.Position += dir * boss.Archetype.MoveSpeed * dt;
        }

        if (boss.BossPhaseTimer > 0f) return;

        RefreshBossAggro(boss, player, npc);
        var strike = GetStrikeFocusWorld(boss, player, npc);
        boss.BossPhase = PickNextAction(boss, strike);
        var firstSlamTeleMul = boss.HasCompletedFirstBossSlam ? 1f : 1.38f;
        boss.BossPhaseTimer = boss.BossPhase switch
        {
            BossAttackPhase.SlamTelegraph => SlamTelegraphTime * PhaseSpeedMul(boss) * firstSlamTeleMul,
            BossAttackPhase.VolleyTelegraph => VolleyTelegraphTime * PhaseSpeedMul(boss),
            BossAttackPhase.Charge => ChargeDuration,
            _ => IdleDuration
        };

        if (boss.BossPhase == BossAttackPhase.SlamTelegraph)
        {
            boss.BossActionCenter = strike;
            boss.BossActionRadius = SlamRadius;
        }
    }

    private static void TickSlamTelegraph(EnemyModel boss)
    {
        if (boss.BossPhaseTimer > 0f) return;
        boss.BossPhase = BossAttackPhase.SlamImpact;
        boss.BossPhaseTimer = 0.12f;
    }

    private static void TickSlamImpact(EnemyModel boss, PlayerModel player, NpcModel npc,
        List<HexagonModel>? destructibleWalls)
    {
        if (Vector2.Distance(boss.BossActionCenter, player.Position) < boss.BossActionRadius)
            player.TakeDamage(SlamDamagePlayer);

        if (!npc.IsPickedUp && !npc.IsDead
            && Vector2.Distance(boss.BossActionCenter, npc.Position) < boss.BossActionRadius)
            npc.TakeDamage(SlamDamageNpc);

        boss.HasCompletedFirstBossSlam = true;

        if (destructibleWalls != null)
        {
            foreach (var w in destructibleWalls)
            {
                if (!w.IsDestructible || w.IsBroken) continue;
                if (Vector2.Distance(w.Center, boss.BossActionCenter) > boss.BossActionRadius + 40f) continue;
                w.DestructibleHealth -= 80f;
                if (w.DestructibleHealth <= 0f && !BrokenWallsThisFrame.Contains(w))
                    BrokenWallsThisFrame.Add(w);
            }
        }

        boss.MuzzleFlashTimer = 0.18f;
        boss.MuzzleFlashDir = Vector2.UnitY;

        if (boss.BossPhaseTimer > 0f) return;
        boss.BossPhase = BossAttackPhase.Idle;
        boss.BossPhaseTimer = IdleDuration * PhaseSpeedMul(boss);
    }

    private static void TickVolleyTelegraph(EnemyModel boss, PlayerModel player, NpcModel npc)
    {
        boss.BossActionCenter = GetStrikeFocusWorld(boss, player, npc);
        if (boss.BossPhaseTimer > 0f) return;
        boss.BossPhase = BossAttackPhase.VolleyFire;
        boss.BossPhaseTimer = 0.08f;
    }

    private static void TickVolleyFire(EnemyModel boss, PlayerModel player, NpcModel npc,
        List<ProjectileModel> projectiles)
    {
        var toStrike = GetStrikeFocusWorld(boss, player, npc) - boss.Position;
        if (toStrike == Vector2.Zero) toStrike = Vector2.UnitX;
        else toStrike.Normalize();

        var aimAngle = (float)Math.Atan2(toStrike.Y, toStrike.X);
        var step = VolleyFanAngle / (VolleyShots - 1);
        var start = aimAngle - VolleyFanAngle * 0.5f;
        var speed = boss.Archetype.ProjectileSpeed;
        var radius = boss.Archetype.ProjectileRadius;

        for (var i = 0; i < VolleyShots; i++)
        {
            var a = start + step * i;
            var dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
            projectiles.Add(new ProjectileModel(boss.Position, dir * speed, radius)
            {
                LifeTime = 2.5f
            });
        }

        boss.MuzzleFlashTimer = 0.22f;
        boss.MuzzleFlashDir = toStrike;

        if (boss.BossPhaseTimer > 0f) return;
        boss.BossPhase = BossAttackPhase.Idle;
        boss.BossPhaseTimer = IdleDuration * PhaseSpeedMul(boss);
    }

    private static void TickCharge(EnemyModel boss, PlayerModel player, NpcModel npc, float dt)
    {
        var focus = GetStrikeFocusWorld(boss, player, npc);
        var dir = focus - boss.Position;
        if (dir != Vector2.Zero)
        {
            dir.Normalize();
            boss.Position += dir * boss.Archetype.MoveSpeed * ChargeSpeedMultiplier * dt;
        }

        if (boss.AttackCooldown <= 0f)
        {
            var melee = boss.Archetype.MeleeReach;
            if (boss.BossFocusOnNpc && !npc.IsPickedUp && !npc.IsDead
                && Vector2.Distance(boss.Position, npc.Position) < melee)
            {
                npc.TakeDamage(ChargeMeleeDamage);
                boss.AttackCooldown = 0.6f;
            }
            else if (!boss.BossFocusOnNpc && Vector2.Distance(boss.Position, player.Position) < melee)
            {
                player.TakeDamage(ChargeMeleeDamage);
                boss.AttackCooldown = 0.6f;
            }
            else if (boss.BossFocusOnNpc && Vector2.Distance(boss.Position, player.Position) < melee * 0.85f)
            {
                // Редкий случай: рвёт к NPC, но зацепил игрока на пути — лёгкий клип.
                player.TakeDamage(ChargeMeleeDamage * 0.45f);
                boss.AttackCooldown = 0.6f;
            }
        }

        if (boss.BossPhaseTimer > 0f) return;
        boss.BossPhase = BossAttackPhase.Idle;
        boss.BossPhaseTimer = IdleDuration * PhaseSpeedMul(boss);
    }

    private static BossAttackPhase PickNextAction(EnemyModel boss, Vector2 strikeWorld)
    {
        var hpFrac = boss.Health / boss.MaxHealth;
        var dist = Vector2.Distance(boss.Position, strikeWorld);

        var rng = new Random(boss.GetHashCode() ^ (int)(boss.BossPhaseTimer * 1000f));
        var roll = rng.NextDouble();

        if (hpFrac <= Phase3HpFraction && dist > 240f && roll < 0.45)
            return BossAttackPhase.Charge;

        if (hpFrac <= Phase2HpFraction && dist > 320f && roll < 0.55)
            return BossAttackPhase.VolleyTelegraph;

        return BossAttackPhase.SlamTelegraph;
    }

    /// <summary>Чем ниже HP — тем быстрее босс отыгрывает паттерны.</summary>
    private static float PhaseSpeedMul(EnemyModel boss)
    {
        var hpFrac = boss.Health / boss.MaxHealth;
        if (hpFrac <= Phase3HpFraction) return 0.55f;
        if (hpFrac <= Phase2HpFraction) return 0.75f;
        return 1f;
    }
}

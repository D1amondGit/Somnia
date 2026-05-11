using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.AI;

/// <summary>Поведение врагов: выбор цели, движение steering + сепарация, стрельба, инфекция.</summary>
public sealed class EnemyAiService
{
    public void Update(
        float dt,
        List<EnemyModel> enemies,
        PlayerModel player,
        NpcModel npc,
        Rectangle playArea,
        List<Vector3> walls,
        List<ProjectileModel> projectiles)
    {
        foreach (var enemy in enemies.Where(x => !x.IsDead))
        {
            HandleInfection(enemy, dt, enemies);
            if (enemy.IsDummy || enemy.StunTimer > 0) continue;

            var target = SelectTarget(enemy, player, npc);
            var dist = Vector2.Distance(enemy.Position, target.Position);

            if (enemy.Type == EnemyType.Shooter)
                HandleShooter(enemy, player, dist, dt, walls, projectiles, enemies);
            else
                HandleMelee(enemy, target, dist, dt, walls, enemies);
        }
    }

    private static void HandleMelee(EnemyModel enemy, AiTarget target, float dist, float dt,
        List<Vector3> walls, List<EnemyModel> all)
    {
        if (dist <= 65f && enemy.AttackCooldown <= 0)
        {
            target.ApplyDamage(10f);
            enemy.AttackCooldown = 1f;
        }
        else if (dist > 50f)
        {
            MoveSmart(enemy, target.Position, dt, baseSpeed: 150f, walls, all);
        }
    }

    private static void HandleShooter(EnemyModel enemy, PlayerModel player, float dist, float dt,
        List<Vector3> walls, List<ProjectileModel> projectiles, List<EnemyModel> all)
    {
        if (dist > 400f) MoveSmart(enemy, player.Position, dt, baseSpeed: 110f, walls, all);

        if (enemy.AttackCooldown > 0 || dist >= 800f) return;
        if (!HasLineOfSight(enemy.Position, player.Position, walls)) return;

        var dir = Vector2.Normalize(player.Position - enemy.Position);
        projectiles.Add(new ProjectileModel(enemy.Position, dir * 450f, 10f));
        enemy.AttackCooldown = 2.2f;
    }

    private static void MoveSmart(EnemyModel enemy, Vector2 target, float dt, float baseSpeed,
        List<Vector3> walls, List<EnemyModel> all)
    {
        var speed = enemy.SlowTimer > 0 ? baseSpeed * 0.3f : baseSpeed;
        var desired = target - enemy.Position;
        if (desired == Vector2.Zero) return;
        desired.Normalize();

        var avoidance = GetAvoidance(enemy.Position, desired, walls);
        var separation = GetSeparation(enemy, all);

        var combined = desired + avoidance * 4f + separation * 1.5f;
        if (combined == Vector2.Zero) return;

        combined.Normalize();
        enemy.Position += combined * speed * dt;
    }

    private static Vector2 GetAvoidance(Vector2 pos, Vector2 lookDir, List<Vector3> walls)
    {
        var force = Vector2.Zero;
        foreach (var w in walls)
        {
            var toWall = pos - new Vector2(w.X, w.Y);
            var dist = toWall.Length();
            var detectR = w.Z + 80f;
            if (!(dist < detectR) || !(dist > 0)) continue;

            var dot = Vector2.Dot(Vector2.Normalize(toWall), lookDir);
            if (dot >= 0) continue;

            var tangent = new Vector2(-toWall.Y, toWall.X);
            if (Vector2.Dot(tangent, lookDir) < 0) tangent = -tangent;
            force += Vector2.Normalize(tangent) * (1f - dist / detectR);
        }

        return force;
    }

    private static Vector2 GetSeparation(EnemyModel self, List<EnemyModel> others)
    {
        var force = Vector2.Zero;
        foreach (var other in others)
        {
            if (other == self || other.IsDead) continue;
            var diff = self.Position - other.Position;
            var len = diff.Length();
            if (len <= 0 || len >= 50f) continue;

            force += Vector2.Normalize(diff) * (1f - len / 50f);
        }

        return force;
    }

    private static bool HasLineOfSight(Vector2 a, Vector2 b, List<Vector3> walls)
    {
        foreach (var w in walls)
        {
            var c = new Vector2(w.X, w.Y);
            var ab = b - a;
            var len2 = ab.LengthSquared();
            if (len2 == 0) continue;

            var t = MathHelper.Clamp(Vector2.Dot(c - a, ab) / len2, 0f, 1f);
            var closest = a + ab * t;
            var diff = closest - c;
            diff.Y /= IsometricView.Squash;
            if (diff.Length() < w.Z - 5f) return false;
        }

        return true;
    }

    private static AiTarget SelectTarget(EnemyModel enemy, PlayerModel player, NpcModel npc)
    {
        if (enemy.Type == EnemyType.Shooter) return AiTarget.ForPlayer(player);

        var npcActive = !npc.IsPickedUp && !npc.IsDead;
        if (player.State == PlayerState.Carrying || !npcActive) return AiTarget.ForPlayer(player);

        return Vector2.Distance(enemy.Position, player.Position) <
               Vector2.Distance(enemy.Position, npc.Position)
            ? AiTarget.ForPlayer(player)
            : AiTarget.ForNpc(npc);
    }

    private static void HandleInfection(EnemyModel enemy, float dt, List<EnemyModel> enemies)
    {
        if (!enemy.IsInfected) return;

        enemy.InfectionTimer -= dt;
        if (enemy.InfectionTimer > 0) return;

        enemy.TakeDamage(25f, enemy.Position, 0f);
        enemy.IsInfected = false;

        foreach (var neighbor in enemies.Where(x => !x.IsDead && !x.IsInfected))
        {
            if (Vector2.Distance(enemy.Position, neighbor.Position) >= 150f) continue;
            neighbor.IsInfected = true;
            neighbor.InfectionTimer = 0.5f;
        }
    }

    /// <summary>Тонкая обёртка над двумя возможными целями, чтобы избежать null и боксинга.</summary>
    private readonly struct AiTarget
    {
        private readonly PlayerModel? _player;
        private readonly NpcModel? _npc;

        private AiTarget(PlayerModel? player, NpcModel? npc)
        {
            _player = player;
            _npc = npc;
        }

        public static AiTarget ForPlayer(PlayerModel player) => new(player, null);

        public static AiTarget ForNpc(NpcModel npc) => new(null, npc);

        public Vector2 Position => _player?.Position ?? _npc!.Position;

        public void ApplyDamage(float amount)
        {
            if (_player != null) _player.TakeDamage(amount);
            else _npc!.TakeDamage(amount);
        }
    }
}

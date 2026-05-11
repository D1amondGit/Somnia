using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Waves;

public sealed class WaveManager
{
    public int CurrentArena { get; private set; }

    public const int ArenaCount = 4;

    /// <summary>Индекс арены босса (последняя).</summary>
    public const int BossArenaIndex = 3;

    private bool _secretMeatGrinder;

    public bool AllArenasCleared => CurrentArena >= ArenaCount;
    public bool IsBossArena => CurrentArena == BossArenaIndex;
    public bool IsSecretMeatGrinder => _secretMeatGrinder;

    private static readonly EnemyType[][] ArenaComposition =
    {
        // Arena 0: знакомим
        new[]
        {
            EnemyType.Melee, EnemyType.Melee, EnemyType.Melee,
            EnemyType.Charger,
            EnemyType.Shooter
        },
        // Arena 1: пресс
        new[]
        {
            EnemyType.Melee, EnemyType.Melee, EnemyType.Melee, EnemyType.Melee,
            EnemyType.Charger, EnemyType.Charger,
            EnemyType.Shooter, EnemyType.Shooter,
            EnemyType.Sniper
        },
        // Arena 2: пик
        new[]
        {
            EnemyType.Melee, EnemyType.Melee, EnemyType.Melee, EnemyType.Melee,
            EnemyType.Charger, EnemyType.Charger, EnemyType.Charger,
            EnemyType.Shooter, EnemyType.Shooter,
            EnemyType.Sniper, EnemyType.Sniper
        },
        // Arena 3: BOSS + стартовая свита (ещё волны — из оркестратора, пока не овертайм).
        new[]
        {
            EnemyType.Boss,
            EnemyType.Melee, EnemyType.Melee,
            EnemyType.Shooter, EnemyType.Shooter,
            EnemyType.Charger,
            EnemyType.Sniper
        }
    };

    public List<EnemyModel> SpawnCurrentWave(int w, int h)
    {
        if (_secretMeatGrinder)
            return SpawnSecretMeatGrinder(w, h);

        var roster = ArenaComposition[Math.Min(CurrentArena, ArenaComposition.Length - 1)];
        var enemies = new List<EnemyModel>(roster.Length);

        if (IsBossArena)
        {
            // Босс — по центру правой половины (читаемо). Миньоны — вокруг.
            var bossCenter = new Vector2(w * 0.7f, h * 0.5f);
            const float minionRingRadius = 400f;
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i] == EnemyType.Boss)
                {
                    enemies.Add(new EnemyModel(bossCenter, EnemyType.Boss));
                    continue;
                }
                var angle = i * MathF.PI * 0.6f;
                var pos = bossCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * minionRingRadius;
                enemies.Add(new EnemyModel(pos, roster[i]));
            }

            foreach (var e in enemies)
            {
                if (e.Type == EnemyType.Boss) continue;
                e.AttackCooldown = MathHelper.Max(e.AttackCooldown, 2.2f);
            }

            return enemies;
        }

        var center = new Vector2(w * 0.8f, h / 2f);
        var radiusH = h * 0.4f;

        for (var i = 0; i < roster.Length; i++)
        {
            var angle = -1.5f + 3f * (roster.Length <= 1 ? 0.5f : i / (float)(roster.Length - 1));
            var pos = center + new Vector2(
                MathF.Cos(angle) * radiusH * 0.5f,
                MathF.Sin(angle) * radiusH);
            enemies.Add(new EnemyModel(pos, roster[i]));
        }

        return enemies;
    }

    /// <summary>Секрет: огромная свалка всех типов. Детерминированно от размеров арены (тесты).</summary>
    private static List<EnemyModel> SpawnSecretMeatGrinder(int w, int h)
    {
        var rnd = new Random(unchecked(w * 7919 + h * 104729 + 0x5ec7e));
        var types = new List<EnemyType>(64);
        void addMany(EnemyType t, int n)
        {
            for (var i = 0; i < n; i++) types.Add(t);
        }

        // Чуть меньше и мягче старт — иначе секрет почти непроходим.
        addMany(EnemyType.Melee, 12);
        addMany(EnemyType.Shooter, 10);
        addMany(EnemyType.Charger, 10);
        addMany(EnemyType.Sniper, 8);

        for (var i = types.Count - 1; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (types[i], types[j]) = (types[j], types[i]);
        }

        var marginX = w * 0.08f;
        var marginY = h * 0.10f;
        var innerW = w - marginX * 2f;
        var innerH = h - marginY * 2f;
        const int cols = 8;
        const int rows = 7;
        var cellW = innerW / cols;
        var cellH = innerH / rows;
        var spawnSafe = new Vector2(250f, h * 0.5f);
        const float spawnSafeRadius = 210f;

        var enemies = new List<EnemyModel>(types.Count);
        var idx = 0;
        for (var r = 0; r < rows && idx < types.Count; r++)
        for (var c = 0; c < cols && idx < types.Count; c++)
        {
            var cx = marginX + cellW * (c + 0.5f) + ((float)rnd.NextDouble() - 0.5f) * cellW * 0.55f;
            var cy = marginY + cellH * (r + 0.5f) + ((float)rnd.NextDouble() - 0.5f) * cellH * 0.55f;
            var pos = new Vector2(cx, cy);
            if (Vector2.Distance(pos, spawnSafe) < spawnSafeRadius) continue;

            enemies.Add(new EnemyModel(pos, types[idx]));
            enemies[^1].AttackCooldown = 1.15f + (float)rnd.NextDouble() * 1.65f;
            idx++;
        }

        for (var guard = 0; idx < types.Count && guard < 800; guard++)
        {
            var pos = new Vector2(
                marginX + (float)rnd.NextDouble() * innerW,
                marginY + (float)rnd.NextDouble() * innerH);
            if (Vector2.Distance(pos, spawnSafe) < spawnSafeRadius - 20f) continue;

            enemies.Add(new EnemyModel(pos, types[idx]));
            enemies[^1].AttackCooldown = 1.15f + (float)rnd.NextDouble() * 1.65f;
            idx++;
        }

        return enemies;
    }

    public void AdvanceArena() => CurrentArena++;

    public void ResetArenaIndex()
    {
        CurrentArena = 0;
        _secretMeatGrinder = false;
    }

    /// <summary>Секретный режим: не трогает индекс основной кампании.</summary>
    public void EnterSecretMeatGrinderMode() => _secretMeatGrinder = true;

    public void ExitSecretMeatGrinderMode() => _secretMeatGrinder = false;

    /// <summary>
    /// Только для отладки: перескочить на арену с боссом (шорткат в <see cref="Game1"/>).
    /// Не использовать в релизной логике прогрессии.
    /// </summary>
    public void DebugJumpToBossArena() => CurrentArena = BossArenaIndex;
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.Waves;

public sealed class WaveManager
{
    public int CurrentArena { get; private set; }

    public const int ArenaCount = 3;

    public bool AllArenasCleared => CurrentArena >= ArenaCount;

    public List<EnemyModel> SpawnCurrentWave(int w, int h)
    {
        var enemies = new List<EnemyModel>();
        var center = new Vector2(w * 0.8f, h / 2f);

        switch (CurrentArena)
        {
            case 0:
                AddEnemyBatch(enemies, 4, EnemyType.Melee, center, h * 0.35f);
                AddEnemyBatch(enemies, 1, EnemyType.Shooter, center, h * 0.2f);
                break;
            case 1:
                AddEnemyBatch(enemies, 6, EnemyType.Melee, center, h * 0.4f);
                AddEnemyBatch(enemies, 2, EnemyType.Shooter, center, h * 0.3f);
                break;
            default:
                AddEnemyBatch(enemies, 8, EnemyType.Melee, center, h * 0.45f);
                AddEnemyBatch(enemies, 3, EnemyType.Shooter, center, h * 0.35f);
                break;
        }

        return enemies;
    }

    public void AdvanceArena() => CurrentArena++;

    public void ResetArenaIndex() => CurrentArena = 0;

    private static void AddEnemyBatch(List<EnemyModel> list, int count, EnemyType type, Vector2 center, float radius)
    {
        for (var i = 0; i < count; i++)
        {
            var angleStep = count <= 1 ? 0f : 3f / (count - 1);
            var angle = -1.5f + angleStep * i;
            var pos = center + new Vector2((float)Math.Cos(angle) * radius * 0.5f, (float)Math.Sin(angle) * radius);
            list.Add(new EnemyModel(pos, type));
        }
    }
}

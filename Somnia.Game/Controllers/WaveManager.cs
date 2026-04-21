using System;
using System.Collections.Generic;
using System.Numerics;
using Somnia.Game.Models;

namespace Somnia.Game.Controllers
{
    public enum EnemyType { Melee, Shooter }

    public class WaveManager
    {
        private int _currentArena;
        public bool WaveJustCleared { get; private set; }
        public const int ArenaCount = 3;

        public static readonly Vector2[] ArenaCenters =
        {
            new Vector2(400, 400),
            new Vector2(1600, 400),
            new Vector2(2800, 400)
        };

        public bool AllArenasCleared => _currentArena >= ArenaCount;

        public List<EnemyModel> SpawnCurrentWave()
        {
            var enemies = new List<EnemyModel>();
            if (_currentArena == 0)
                AddEnemies(enemies, 3, EnemyType.Melee, ArenaCenters[0]);
            else if (_currentArena == 1)
            {
                AddEnemies(enemies, 2, EnemyType.Melee, ArenaCenters[1]);
                AddEnemies(enemies, 1, EnemyType.Shooter, ArenaCenters[1]);
            }
            else if (_currentArena == 2)
            {
                AddEnemies(enemies, 3, EnemyType.Melee, ArenaCenters[2]);
                AddEnemies(enemies, 2, EnemyType.Shooter, ArenaCenters[2]);
            }
            WaveJustCleared = false;
            return enemies;
        }

        public void CheckWaveCleared(List<EnemyModel> enemies)
        {
            if (enemies.Count == 0 || !enemies.TrueForAll(e => e.IsDead)) return;
            if (_currentArena < ArenaCount) _currentArena++;
            WaveJustCleared = true;
        }

        private void AddEnemies(List<EnemyModel> list, int count, EnemyType type, Vector2 center)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = i * (MathF.PI * 2f / count);
                Vector2 pos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 150f;
                list.Add(CreateEnemy(pos, type));
            }
        }

        private EnemyModel CreateEnemy(Vector2 pos, EnemyType type)
        {
            var e = new EnemyModel(pos);
            if (type == EnemyType.Shooter) { e.Speed = 80f; e.AttackRadius = 200f; e.Damage = 8f; }
            return e;
        }
    }
}

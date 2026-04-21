using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using System.Linq;

namespace Somnia.Game.Controllers
{
    public class WaveManager
    {
        private int _currentArena;
        public bool WaveJustCleared { get; private set; }
        public const int ArenaCount = 3;

        public static readonly List<Vector2> ArenaCenters = new List<Vector2> {
            new Vector2(400, 400), new Vector2(1600, 400), new Vector2(2800, 400)
        };

        public bool AllArenasCleared => _currentArena >= ArenaCount;

        public List<EnemyModel> SpawnCurrentWave()
        {
            var enemies = new List<EnemyModel>();
            if (_currentArena == 0) AddEnemies(enemies, 3, EnemyType.Melee, ArenaCenters.ElementAt(0));
            else if (_currentArena == 1) {
                AddEnemies(enemies, 2, EnemyType.Melee, ArenaCenters.ElementAt(1));
                AddEnemies(enemies, 1, EnemyType.Shooter, ArenaCenters.ElementAt(1));
            }
            else if (_currentArena == 2) {
                AddEnemies(enemies, 3, EnemyType.Melee, ArenaCenters.ElementAt(2));
                AddEnemies(enemies, 2, EnemyType.Shooter, ArenaCenters.ElementAt(2));
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
            for (int i = 0; i < count; i++) {
                float angle = i * (MathF.PI * 2f / count);
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 150f;
                list.Add(new EnemyModel(pos, type));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Controllers
{
    public class WaveManager
    {
        public int CurrentArena { get; private set; }
        public const int ArenaCount = 3;
        public bool AllArenasCleared => CurrentArena >= ArenaCount;

        public List<EnemyModel> SpawnCurrentWave(int w, int h)
        {
            var enemies = new List<EnemyModel>();
            Vector2 c = new Vector2(w * 0.8f, h / 2f); // Точка спавна смещена вправо
            
            if (CurrentArena == 0) {
                AddEnemies(enemies, 4, EnemyType.Melee, c, h * 0.35f);
                AddEnemies(enemies, 1, EnemyType.Shooter, c, h * 0.2f);
            } else if (CurrentArena == 1) {
                AddEnemies(enemies, 6, EnemyType.Melee, c, h * 0.4f);
                AddEnemies(enemies, 2, EnemyType.Shooter, c, h * 0.3f);
            } else {
                AddEnemies(enemies, 8, EnemyType.Melee, c, h * 0.45f);
                AddEnemies(enemies, 3, EnemyType.Shooter, c, h * 0.35f);
            }
            return enemies;
        }

        public void AdvanceArena() => CurrentArena++;

        private void AddEnemies(List<EnemyModel> list, int count, EnemyType type, Vector2 center, float radius)
        {
            for (int i = 0; i < count; i++) {
                // Выстраиваем их полукругом (от -90 до +90 градусов)
                float angle = -1.5f + (3f / Math.Max(1, count - 1)) * i;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle) * radius * 0.5f, (float)Math.Sin(angle) * radius);
                list.Add(new EnemyModel(pos, type));
            }
        }
    }
}
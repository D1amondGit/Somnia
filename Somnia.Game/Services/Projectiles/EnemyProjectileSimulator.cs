using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.Particles;

namespace Somnia.Game.Services.Projectiles;

/// <summary>Обновление снарядов врагов и столкновения с игроком/NPC.</summary>
public sealed class EnemyProjectileSimulator
{
    private int _hitsOnPlayerThisFrame;
    private int _hitsOnNpcThisFrame;

    /// <summary>Сколько вражеских снарядов попало в игрока в последнем кадре.</summary>
    public int ConsumeHitsOnPlayer()
    {
        var h = _hitsOnPlayerThisFrame;
        _hitsOnPlayerThisFrame = 0;
        return h;
    }

    public int ConsumeHitsOnNpc()
    {
        var h = _hitsOnNpcThisFrame;
        _hitsOnNpcThisFrame = 0;
        return h;
    }

    public void Update(float dt, List<ProjectileModel> projectiles, PlayerModel player, NpcModel npc,
        FloorEffectService? fx = null, List<FloorSplatter>? splatters = null,
        bool skipDamageToPlayerAndNpc = false)
    {
        _hitsOnPlayerThisFrame = 0;
        _hitsOnNpcThisFrame = 0;

        for (var i = projectiles.Count - 1; i >= 0; i--)
        {
            var pr = projectiles[i];
            pr.Position += pr.Velocity * dt;
            pr.LifeTime -= dt;
            if (pr.LifeTime <= 0f)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            if (!skipDamageToPlayerAndNpc && Vector2.Distance(pr.Position, player.Position) < 30f)
            {
                player.TakeDamage(12f);
                _hitsOnPlayerThisFrame++;
                if (fx != null && splatters != null)
                    fx.EmitImpact(splatters, player.Position, new Color(220, 50, 60), 14, 180f);
                projectiles.RemoveAt(i);
            }
            else if (!skipDamageToPlayerAndNpc && !npc.IsPickedUp && Vector2.Distance(pr.Position, npc.Position) < 30f)
            {
                npc.TakeDamage(10f);
                _hitsOnNpcThisFrame++;
                if (fx != null && splatters != null)
                    fx.EmitImpact(splatters, npc.Position, new Color(255, 180, 80), 10, 150f);
                projectiles.RemoveAt(i);
            }
        }
    }
}

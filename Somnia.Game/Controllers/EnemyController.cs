using System;
using System.Numerics;
using Somnia.Game.Models;

namespace Somnia.Game.Controllers
{
    public class EnemyController
    {
        private EnemyModel _model;
        private const float PlayerAggroRange = 200f;
        private const float NpcAggroRange = 300f;
        private const float AttackDistanceBuffer = 10f;

        public EnemyController(EnemyModel model) => _model = model;

        public void Update(
            float deltaTime,
            PlayerModel player,
            NpcModel npc,
            int screenWidth,
            int screenHeight)
        {
            if (_model.IsDead) return;

            Vector2? target = SelectTarget(player, npc);
            if (target == null) return;

            MoveToward(target.Value, deltaTime, screenWidth, screenHeight);
            TryAttack(target.Value, player, npc);
        }

        private Vector2? SelectTarget(PlayerModel player, NpcModel npc)
        {
            float playerDist = DistanceTo(player.Position);
            if (playerDist <= PlayerAggroRange)
                return player.Position;

            if (!npc.IsPickedUp && !npc.IsDead)
            {
                float npcDist = DistanceTo(npc.Position);
                if (npcDist <= NpcAggroRange)
                    return npc.Position;
            }

            return null;
        }

        private void MoveToward(Vector2 target, float dt, int sw, int sh)
        {
            Vector2 dir = target - _model.Position;
            if (dir.LengthSquared() < 1f) return;

            Vector2 normalized = Vector2.Normalize(dir);
            _model.Position += normalized * _model.Speed * dt;
            ClampToScreen(sw, sh);
        }

        private void TryAttack(Vector2 target, PlayerModel player, NpcModel npc)
        {
            float dist = DistanceTo(target);
            float range = _model.AttackRadius + AttackDistanceBuffer;
            if (dist > range || !_model.CanAttack()) return;

            _model.PerformAttack();
            DealDamage(player, npc);
        }

        private void DealDamage(PlayerModel player, NpcModel npc)
        {
            float playerDist = DistanceTo(player.Position);
            if (playerDist <= _model.AttackRadius + AttackDistanceBuffer)
            {
                player.TakeDamage(_model.Damage);
            }

            if (!npc.IsPickedUp && !npc.IsDead)
            {
                float npcDist = DistanceTo(npc.Position);
                if (npcDist <= _model.AttackRadius + AttackDistanceBuffer)
                    npc.TakeDamage(_model.Damage);
            }
        }

        private float DistanceTo(Vector2 point) =>
            Vector2.Distance(_model.Position, point);

        private void ClampToScreen(int sw, int sh)
        {
            _model.Position = new Vector2(
                (float)Math.Max(0, Math.Min(_model.Position.X, sw - 40)),
                (float)Math.Max(0, Math.Min(_model.Position.Y, sh - 40)));
        }
    }
}

using System;
using System.Numerics;
using System.Collections.Generic;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }
    public enum GameState { Playing, Paused, GameOver }
    public enum AnomalyType { Red, Blue, Neutral }

    public class PlayerModel
    {
        public Vector2 Position { get; set; }
        public PlayerState State { get; private set; } = PlayerState.Free;
        public AnomalyType CurrentZone { get; set; } = AnomalyType.Neutral;
        public Vector2 FacingDirection { get; private set; } = Vector2.UnitX;

        public float CurrentHealth { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public float DamageMultiplier { get; set; } = 1.0f;
        public bool IsDead => CurrentHealth <= 0;
        public bool IsDashing { get; private set; }
        public bool IsAttacking => _attackTimer > 0;

        private float _attackTimer, _skillCooldown, _dashTimer;
        private Vector2 _dashDir;
        private const float BaseSpeed = 500f;

        public PlayerModel(Vector2 start) => Position = start;

        public void SetState(PlayerState s) => State = s;
        public void UpdateFacing(Vector2 dir) => FacingDirection = dir == Vector2.Zero ? FacingDirection : Vector2.Normalize(dir);
        public void TakeDamage(float amt) => CurrentHealth = Math.Max(0, CurrentHealth - amt);

        public void Move(Vector2 dir, float dt, int w, int h)
        {
            UpdateTimers(dt);
            Vector2 move = IsDashing ? _dashDir * BaseSpeed * 4 : (dir == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(dir) * GetSpeed());
            Position = ClampPos(Position + move * dt, w, h);
        }

        private void UpdateTimers(float dt)
        {
            if (_attackTimer > 0) _attackTimer -= dt;
            if (_skillCooldown > 0) _skillCooldown -= dt;
            if (_dashTimer > 0) { _dashTimer -= dt; if (_dashTimer <= 0) IsDashing = false; }
        }

        private float GetSpeed() => State == PlayerState.Carrying ? BaseSpeed * 0.5f : BaseSpeed;
        private Vector2 ClampPos(Vector2 pos, int w, int h) => new Vector2(Math.Clamp(pos.X, 0, w - 50), Math.Clamp(pos.Y, 0, h - 50));

        public void StartDash(Vector2 dir)
        {
            if (State == PlayerState.Free && dir != Vector2.Zero) { IsDashing = true; _dashTimer = 0.15f; _dashDir = Vector2.Normalize(dir); }
        }

        public void UsePrimary(Vector2 toMouse, List<EnemyModel> enemies)
        {
            if (_skillCooldown > 0 || State == PlayerState.Carrying) return;
            if (CurrentZone == AnomalyType.Red) RedShotgun(toMouse, enemies);
            else NeutralAttack(toMouse, enemies);
            _skillCooldown = 0.5f; _attackTimer = 0.15f;
        }

        public void UseSecondary(Vector2 toMouse, List<EnemyModel> enemies)
        {
            if (_skillCooldown > 0 || CurrentZone != AnomalyType.Red) return;
            RedLasso(toMouse, enemies);
            _skillCooldown = 0.8f;
        }

        private void RedShotgun(Vector2 dir, List<EnemyModel> enemies)
        {
            var nDir = Vector2.Normalize(dir);
            foreach (var e in enemies)
            {
                Vector2 toE = e.Position - Position;
                if (toE.Length() < 180f && Vector2.Dot(nDir, Vector2.Normalize(toE)) > 0.5f)
                    e.TakeDamage(35f * DamageMultiplier, Position, 900f);
            }
        }

        private void RedLasso(Vector2 dir, List<EnemyModel> enemies)
        {
            foreach (var e in enemies)
                if (Vector2.Distance(Position, e.Position) < 400f)
                    e.Position = Vector2.Lerp(e.Position, Position, 0.7f);
        }

        private void NeutralAttack(Vector2 dir, List<EnemyModel> enemies)
        {
            var nDir = Vector2.Normalize(dir == Vector2.Zero ? FacingDirection : dir);
            foreach (var e in enemies)
            {
                Vector2 toE = e.Position - Position;
                if (toE.Length() < 120f && Vector2.Dot(nDir, Vector2.Normalize(toE)) > 0.3f)
                    e.TakeDamage(20f * DamageMultiplier, Position, 400f);
            }
        }
    }
}

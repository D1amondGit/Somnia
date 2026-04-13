using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }

    public enum GameState { Playing, Paused, GameOver }

    public class NpcModel
    {
        public Vector2 Position { get; set; }
        public bool IsPickedUp { get; set; }
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public NpcModel(Vector2 position)
        {
            CurrentHealth = MaxHealth;
            Position = position;
            IsPickedUp = false;
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
        }
    }

    public class PlayerModel
    {
        public Vector2 Position { get; set; }
        public PlayerState State { get; private set; }
        public float BaseSpeed { get; set; } = 500f;
        public float CurrentSpeed => State == PlayerState.Carrying
            ? BaseSpeed * 0.5f : BaseSpeed;
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        public bool IsDashing { get; private set; }
        public Vector2 AttackDirection { get; private set; }
        public bool IsAttacking { get; private set; }
        public float AttackVisualTimer { get; private set; }

        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _attackCooldownTimer;
        private ZoneType _currentZone;

        private const float DashSpeedMultiplier = 4f;
        private const float DashDuration = 0.15f;
        private const float DashCooldown = 1.5f;
        private const float AttackVisualDuration = 0.15f;
        private Vector2 _dashDirection;

        public bool IsDead => CurrentHealth <= 0;

        public PlayerModel(Vector2 startPosition)
        {
            Position = startPosition;
            State = PlayerState.Free;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
        }

        public void SetState(PlayerState newState) => State = newState;

        public void StartDash(Vector2 direction)
        {
            if (_dashCooldownTimer > 0 || IsDashing) return;
            if (direction == Vector2.Zero) return;
            if (State != PlayerState.Free) return;

            IsDashing = true;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
            _dashDirection = Vector2.Normalize(direction);
        }

        public void StartAttack(Vector2 direction, AnomalyZone zone)
        {
            if (State == PlayerState.Carrying) return;
            if (direction == Vector2.Zero) return;
            if (_attackCooldownTimer > 0) return;

            _currentZone = zone?.Type ?? ZoneType.Neutral;
            AttackDirection = Vector2.Normalize(direction);
            IsAttacking = true;
            AttackVisualTimer = AttackVisualDuration;
            _attackCooldownTimer = GetZoneCooldown(_currentZone);
        }

        public float GetAttackDamage()
        {
            return GetZoneDamage(_currentZone);
        }

        private static float GetZoneDamage(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Red => 25f,
                ZoneType.Green => 15f,
                ZoneType.Blue => 8f,
                _ => 15f,
            };
        }

        private static float GetZoneCooldown(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Red => 1.2f,
                ZoneType.Green => 0.6f,
                ZoneType.Blue => 0.25f,
                _ => 0.6f,
            };
        }

        public float GetAttackRange()
        {
            return _currentZone switch
            {
                ZoneType.Red => 100f,
                ZoneType.Green => 70f,
                ZoneType.Blue => 50f,
                _ => 50f,
            };
        }

        public float GetConeHalfAngle()
        {
            return _currentZone switch
            {
                ZoneType.Red => MathF.PI / 4f,
                ZoneType.Green => MathF.PI / 3f,
                ZoneType.Blue => MathF.PI / 2.4f,
                _ => MathF.PI / 3f,
            };
        }

        public bool IsPointInAttackCone(Vector2 targetPoint)
        {
            if (!IsAttacking) return false;
            Vector2 toTarget = targetPoint - Position;
            float dist = toTarget.Length();
            float range = GetAttackRange();
            if (dist > range || dist < 1f) return false;
            return CheckAngle(toTarget, dist);
        }

        private bool CheckAngle(Vector2 toTarget, float dist)
        {
            Vector2 norm = toTarget / dist;
            float dot = Vector2.Dot(AttackDirection, norm);
            return dot >= MathF.Cos(GetConeHalfAngle());
        }

        public void Move(Vector2 direction, float deltaTime, int sw, int sh)
        {
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= deltaTime;
            UpdateDashState(deltaTime);
            UpdateAttackTimers(deltaTime);

            Vector2 move = ComputeMovement(direction, deltaTime);
            if (move == Vector2.Zero) return;
            Position = ClampPosition(Position + move, sw, sh);
        }

        private void UpdateDashState(float deltaTime)
        {
            if (!IsDashing) return;
            _dashTimer -= deltaTime;
            if (_dashTimer <= 0) IsDashing = false;
        }

        private void UpdateAttackTimers(float deltaTime)
        {
            if (_attackCooldownTimer > 0)
                _attackCooldownTimer -= deltaTime;
            if (AttackVisualTimer <= 0) return;
            AttackVisualTimer -= deltaTime;
            if (AttackVisualTimer <= 0) IsAttacking = false;
        }

        private Vector2 ComputeMovement(Vector2 direction, float deltaTime)
        {
            if (IsDashing)
                return _dashDirection * BaseSpeed
                    * DashSpeedMultiplier * deltaTime;
            if (direction == Vector2.Zero) return Vector2.Zero;
            return Vector2.Normalize(direction) * CurrentSpeed * deltaTime;
        }

        private static Vector2 ClampPosition(Vector2 pos, int sw, int sh)
        {
            return new Vector2(
                Math.Clamp(pos.X, 0, sw - 50),
                Math.Clamp(pos.Y, 0, sh - 50));
        }
    }
}

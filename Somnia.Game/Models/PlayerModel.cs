using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }

    // НОВОЕ: Глобальные состояния игры
    public enum GameState { Playing, Paused, GameOver }

    // НОВОЕ: Направление взгляда игрока для атаки
    public enum FacingDirection { Up, Down, Left, Right }

    public class NpcModel
    {
        public Vector2 Position { get; set; }
        public bool IsPickedUp { get; set; }
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        
        // НОВОЕ: Проверка на смерть
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
        public float CurrentSpeed => State == PlayerState.Carrying ? BaseSpeed * 0.5f : BaseSpeed;
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        public bool IsDashing { get; private set; }
        public FacingDirection FacingDirection { get; set; } = FacingDirection.Down;
        public bool IsAttacking { get; private set; }
        public float AttackVisualTimer { get; private set; }
        private float _dashTimer = 0f;
        private float _dashCooldownTimer = 0f;
        private float _attackCooldownTimer = 0f;
        private ZoneType _currentZone = ZoneType.Neutral;

        private const float DashSpeedMultiplier = 4f; // Во сколько раз быстрее летим
        private const float DashDuration = 0.15f;    // Длительность рывка
        private const float DashCooldown = 1.5f;     // Куладаун
        private const float AttackVisualDuration = 0.15f;
        private Vector2 _dashDirection;
        
        public void StartDash(Vector2 direction)
        {
            // Нельзя рывком выйти из состояния Carrying (по логике игры)
            // и нельзя использовать, если кулдаун не прошел
            if (_dashCooldownTimer <= 0 && !IsDashing && direction != Vector2.Zero && State == PlayerState.Free)
            {
                IsDashing = true;
                _dashTimer = DashDuration;
                _dashCooldownTimer = DashCooldown;
                _dashDirection = Vector2.Normalize(direction);
            }
        }

        public void UpdateAttackTimers(float deltaTime)
        {
            if (_attackCooldownTimer > 0)
                _attackCooldownTimer -= deltaTime;
            if (AttackVisualTimer > 0)
            {
                AttackVisualTimer -= deltaTime;
                if (AttackVisualTimer <= 0)
                    IsAttacking = false;
            }
        }

        public void StartAttack(AnomalyZone zone)
        {
            if (State == PlayerState.Carrying) return;
            _currentZone = zone?.Type ?? ZoneType.Neutral;
            if (_attackCooldownTimer > 0) return;
            IsAttacking = true;
            AttackVisualTimer = AttackVisualDuration;
            _attackCooldownTimer = GetZoneCooldown(_currentZone);
        }

        public float GetAttackDamage() => GetZoneDamage(_currentZone);

        private static float GetZoneDamage(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Red => 25f,
                ZoneType.Blue => 8f,
                _ => 15f,
            };
        }

        private static float GetZoneCooldown(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Red => 1.2f,
                ZoneType.Blue => 0.25f,
                _ => 0.6f,
            };
        }

        public float GetAttackRange()
        {
            return _currentZone switch
            {
                ZoneType.Red => 80f,
                ZoneType.Blue => 60f,
                _ => 50f,
            };
        }

        public Vector2 GetAttackDirectionVector()
        {
            return FacingDirection switch
            {
                FacingDirection.Up => -Vector2.UnitY,
                FacingDirection.Down => Vector2.UnitY,
                FacingDirection.Left => -Vector2.UnitX,
                _ => Vector2.UnitX,
            };
        }

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

        public void Move(Vector2 direction, float deltaTime, int screenWidth, int screenHeight)
        {
            // Обновляем таймеры
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= deltaTime;
            UpdateAttackTimers(deltaTime);

            Vector2 moveVector;

            if (IsDashing)
            {
                _dashTimer -= deltaTime;
                if (_dashTimer <= 0) IsDashing = false;
            
                // Во время рывка летим с огромной скоростью
                moveVector = _dashDirection * BaseSpeed * DashSpeedMultiplier * deltaTime;
            }
            else
            {
                if (direction == Vector2.Zero) return;
                var normalizedDirection = Vector2.Normalize(direction);
                moveVector = normalizedDirection * CurrentSpeed * deltaTime;
            }

            Vector2 nextPosition = Position + moveVector;

            // Ограничение границами экрана
            nextPosition.X = Math.Clamp(nextPosition.X, 0, screenWidth - 50);
            nextPosition.Y = Math.Clamp(nextPosition.Y, 0, screenHeight - 50);

            Position = nextPosition;
        }
    }
}
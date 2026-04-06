using System;
using System.Numerics;

namespace Somnia.Game.Models
{
    public enum PlayerState { Free, Carrying }

    // Простая модель для NPC
    public class NpcModel
    {
        public Vector2 Position { get; set; }
        public bool IsPickedUp { get; set; }
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }

        public NpcModel(Vector2 position)
        {
            CurrentHealth = MaxHealth;
            Position = position;
            IsPickedUp = false;
        }
    }

    public class PlayerModel
    {
        public Vector2 Position { get; set; }
        public PlayerState State { get; private set; }
        // Увеличили базовую скорость до 500
        public float BaseSpeed { get; set; } = 500f; 
        public float CurrentSpeed => State == PlayerState.Carrying ? BaseSpeed * 0.5f : BaseSpeed;
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }

        public PlayerModel(Vector2 startPosition)
        {
            Position = startPosition;
            State = PlayerState.Free;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount); // Здоровье не падает ниже 0
        }
        public void SetState(PlayerState newState) => State = newState;

        public void Move(Vector2 direction, float deltaTime, int screenWidth, int screenHeight)
        {
            if (direction == Vector2.Zero) return;

            var normalizedDirection = Vector2.Normalize(direction);
            Vector2 nextPosition = Position + normalizedDirection * CurrentSpeed * deltaTime;

            // Ограничение движения границами экрана (с учетом размера квадрата 50x50)
            nextPosition.X = Math.Clamp(nextPosition.X, 0, screenWidth - 50);
            nextPosition.Y = Math.Clamp(nextPosition.Y, 0, screenHeight - 50);

            Position = nextPosition;
        }
    }
}
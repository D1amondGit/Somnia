using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using System;
using System.Numerics;

namespace Somnia.Game.Controllers
{
    public class PlayerController
    {
        private PlayerModel _model;

        public PlayerController(PlayerModel model)
        {
            _model = model;
        }

        public void Update(float deltaTime, int screenWidth, int screenHeight)
        {
            var keyboardState = Keyboard.GetState();
            var direction = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S)) direction.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A)) direction.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D)) direction.X += 1;

            UpdateFacingDirection(direction);

            // Считываем рывок
            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                _model.StartDash(direction);
            }

            _model.Move(direction, deltaTime, screenWidth, screenHeight);
        }

        private void UpdateFacingDirection(Vector2 direction)
        {
            if (direction == Vector2.Zero) return;

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                _model.FacingDirection = direction.X > 0
                    ? FacingDirection.Right
                    : FacingDirection.Left;
            }
            else
            {
                _model.FacingDirection = direction.Y > 0
                    ? FacingDirection.Down
                    : FacingDirection.Up;
            }
        }
    }
}
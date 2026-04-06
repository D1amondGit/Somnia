using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
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

            // Теперь контроллер просто передает направление и размеры экрана
            _model.Move(direction, deltaTime, screenWidth, screenHeight);
        }
    }
}
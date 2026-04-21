using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Controllers
{
    public class PlayerController
    {
        private PlayerModel _model;
        private MouseState _prevM;
        private KeyboardState _prevK;

        public PlayerController(PlayerModel model) => _model = model;

        public void Update(float dt, int w, int h, List<EnemyModel> enemies, Matrix cam)
        {
            var ms = Mouse.GetState();
            var ks = Keyboard.GetState();
            
            Vector2 worldM = Vector2.Transform(new Vector2(ms.X, ms.Y), Matrix.Invert(cam));
            Vector2 toMouse = worldM - _model.Position;
            _model.UpdateFacing(toMouse);

            if (ks.IsKeyDown(Keys.LeftShift)) _model.StartDash(GetDir(ks));
            HandleInput(ms, ks, toMouse, enemies);
            
            _model.Move(GetDir(ks), dt, w, h);
            _prevM = ms; _prevK = ks;
        }

        private void HandleInput(MouseState ms, KeyboardState ks, Vector2 toMouse, List<EnemyModel> enemies)
        {
            if (ms.LeftButton == ButtonState.Pressed && _prevM.LeftButton == ButtonState.Released)
                _model.UsePrimary(toMouse, enemies);
            if (ms.RightButton == ButtonState.Pressed && _prevM.RightButton == ButtonState.Released)
                _model.UseSecondary(toMouse, enemies);
        }

        private Vector2 GetDir(KeyboardState s)
        {
            var d = Vector2.Zero;
            if (s.IsKeyDown(Keys.W)) d.Y -= 1;
            if (s.IsKeyDown(Keys.S)) d.Y += 1;
            if (s.IsKeyDown(Keys.A)) d.X -= 1;
            if (s.IsKeyDown(Keys.D)) d.X += 1;
            return d;
        }
    }
}
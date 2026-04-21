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

        public void Update(float dt, int mapW, int mapH, List<EnemyModel> enemies, Matrix cam, NpcModel npc, List<HexagonModel> walls)
        {
            var ms = Mouse.GetState();
            var ks = Keyboard.GetState();
            
            Vector2 worldM = Vector2.Transform(new Vector2(ms.X, ms.Y), Matrix.Invert(cam));
            Vector2 toMouse = worldM - _model.Position;
            _model.UpdateFacing(toMouse);

            if (ks.IsKeyDown(Keys.LeftShift) && _prevK.IsKeyUp(Keys.LeftShift) && _model.State == PlayerState.Free) 
                _model.StartDash();
            
            if (ks.IsKeyDown(Keys.D1)) _model.ActiveSlot = 0;
            if (ks.IsKeyDown(Keys.D2)) _model.ActiveSlot = 1;
            if (ks.IsKeyDown(Keys.D3)) _model.ActiveSlot = 2;

            if (ms.LeftButton == ButtonState.Pressed && _prevM.LeftButton == ButtonState.Released)
                _model.UseActiveSkill(toMouse, enemies, npc, walls);
            
            float speed = _model.State == PlayerState.Carrying ? 150f : 300f;
            if (_model.IsDashing) speed = 800f;

            Vector2 dir = GetDir(ks);
            if (dir != Vector2.Zero) {
                dir.Normalize();
                _model.Position += dir * speed * dt;
            }

            _model.Position.X = MathHelper.Clamp(_model.Position.X, 100, mapW - 100);
            _model.Position.Y = MathHelper.Clamp(_model.Position.Y, 100, mapH - 100);

            _prevM = ms; _prevK = ks;
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
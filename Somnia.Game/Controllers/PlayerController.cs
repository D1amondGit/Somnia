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

        public void Update(float dt, int w, int h, List<EnemyModel> enemies, Matrix cam, NpcModel npc)
        {
            var ms = Mouse.GetState();
            var ks = Keyboard.GetState();
            
            Vector2 worldM = Vector2.Transform(new Vector2(ms.X, ms.Y), Matrix.Invert(cam));
            Vector2 toMouse = worldM - _model.Position;
            _model.UpdateFacing(toMouse);

            // ПЕРЕКЛЮЧЕНИЕ СЛОТОВ
            if (ks.IsKeyDown(Keys.D1)) _model.ActiveSlot = 0;
            if (ks.IsKeyDown(Keys.D2)) _model.ActiveSlot = 1;
            if (ks.IsKeyDown(Keys.D3)) _model.ActiveSlot = 2;

            if (ms.LeftButton == ButtonState.Pressed && _prevM.LeftButton == ButtonState.Released)
                _model.UseActiveSkill(toMouse, enemies);

            HandleInteraction(ks, npc);
            
            _model.Move(GetDir(ks), dt, w, h);
            _prevM = ms; 
            _prevK = ks;
        }

        private void HandleInteraction(KeyboardState ks, NpcModel npc)
        {
            if (ks.IsKeyDown(Keys.E) && _prevK.IsKeyUp(Keys.E))
            {
                if (_model.State == PlayerState.Free && Vector2.Distance(_model.Position, npc.Position) < 80f)
                { npc.IsPickedUp = true; _model.SetState(PlayerState.Carrying); }
                else if (_model.State == PlayerState.Carrying)
                { npc.IsPickedUp = false; npc.Position = _model.Position + new Vector2(60, 0); _model.SetState(PlayerState.Free); }
            }
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using System.Numerics;

namespace Somnia.Game.Controllers
{
    public class PlayerController
    {
        private PlayerModel _model;

        public PlayerController(PlayerModel model) => _model = model;

        public void Update(float dt, int sw, int sh)
        {
            var kb = Keyboard.GetState();
            var dir = GetMovementDirection(kb);
            HandleDash(kb, dir);
            _model.Move(dir, dt, sw, sh);
        }

        public void ProcessAttack(
            MouseState mouse, MouseState prevMouse,
            Matrix camera, AnomalyZone zone)
        {
            if (!IsMouseClicked(mouse, prevMouse)) return;
            System.Numerics.Vector2 dir =
                ComputeAttackDirection(mouse, camera);
            _model.StartAttack(dir, zone);
        }

        private bool IsMouseClicked(MouseState cur, MouseState prev)
        {
            return cur.LeftButton == ButtonState.Pressed
                && prev.LeftButton == ButtonState.Released;
        }

        private System.Numerics.Vector2 ComputeAttackDirection(
            MouseState mouse, Matrix camera)
        {
            System.Numerics.Vector2 worldMouse =
                GetWorldMousePos(mouse, camera);
            System.Numerics.Vector2 dir =
                worldMouse - _model.Position;
            if (dir.LengthSquared() < 1f)
                return System.Numerics.Vector2.Zero;
            return System.Numerics.Vector2.Normalize(dir);
        }

        private static System.Numerics.Vector2 GetWorldMousePos(
            MouseState mouse, Matrix camera)
        {
            var xnaPos = new Microsoft.Xna.Framework.Vector2(
                mouse.Position.X, mouse.Position.Y);
            var xnaWorld = Microsoft.Xna.Framework.Vector2.Transform(
                xnaPos, Matrix.Invert(camera));
            return new System.Numerics.Vector2(
                xnaWorld.X, xnaWorld.Y);
        }

        private static System.Numerics.Vector2 GetMovementDirection(
            KeyboardState kb)
        {
            var dir = System.Numerics.Vector2.Zero;
            if (kb.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (kb.IsKeyDown(Keys.S)) dir.Y += 1;
            if (kb.IsKeyDown(Keys.A)) dir.X -= 1;
            if (kb.IsKeyDown(Keys.D)) dir.X += 1;
            return dir;
        }

        private void HandleDash(
            KeyboardState kb, System.Numerics.Vector2 dir)
        {
            if (kb.IsKeyDown(Keys.LeftShift))
                _model.StartDash(dir);
        }
    }
}

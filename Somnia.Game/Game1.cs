using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using Somnia.Game.Views;
using System.Collections.Generic;

namespace Somnia.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyboardState _prevKeyboard;
        private MouseState _prevMouse;
        private PlayerModel _playerModel;
        private NpcModel _npcModel;
        private EnemyModel _enemyModel;
        private PlayerController _playerController;
        private EnemyController _enemyController;
        private PlayerView _view;
        private Rectangle _damageZone;
        private List<AnomalyZone> _anomalyZones;
        private Matrix _camera;
        private float _dps = 10f;
        private SpriteFont _font;
        private GameState _gameState = GameState.Playing;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _anomalyZones = new List<AnomalyZone>();
            _camera = Matrix.Identity;
            RestartGame();
            _damageZone = new Rectangle(400, 300, 300, 300);
            SetupAnomalyZones();
            base.Initialize();
        }

        private void SetupAnomalyZones()
        {
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight + 4000;
            int third = w / 3;
            int gap = 20;
            int y = -2000;

            _anomalyZones.Add(new AnomalyZone(
                new Rectangle(0, y, third - gap / 2, h), ZoneType.Red));

            _anomalyZones.Add(new AnomalyZone(
                new Rectangle(third + gap / 2, y, third - gap, h),
                ZoneType.Green));

            _anomalyZones.Add(new AnomalyZone(
                new Rectangle(2 * third + gap / 2, y,
                    third - gap / 2 + 200, h), ZoneType.Blue));
        }

        private void RestartGame()
        {
            _playerModel = new PlayerModel(
                new System.Numerics.Vector2(200, 200));
            _npcModel = new NpcModel(
                new System.Numerics.Vector2(200, 400));
            _enemyModel = new EnemyModel(
                new System.Numerics.Vector2(800, 300));
            _playerController = new PlayerController(_playerModel);
            _enemyController = new EnemyController(_enemyModel);
            _gameState = GameState.Playing;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new PlayerView(GraphicsDevice);
            _font = Content.Load<SpriteFont>("MainFont");
        }

        private AnomalyZone GetPlayerZone()
        {
            foreach (var z in _anomalyZones)
                if (z.ContainsPoint(_playerModel.Position)) return z;
            return null;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int sw = _graphics.PreferredBackBufferWidth;
            int sh = _graphics.PreferredBackBufferHeight;

            if (_gameState == GameState.Playing)
                HandlePlaying(dt, sw, sh, kb, mouse);
            else if (_gameState == GameState.Paused)
                HandlePaused(kb, mouse, sw);
            else if (_gameState == GameState.GameOver)
                HandleGameOver(mouse, sw);

            _prevKeyboard = kb;
            _prevMouse = mouse;
            base.Update(gameTime);
        }

        private void HandlePlaying(float dt, int sw, int sh,
            KeyboardState kb, MouseState mouse)
        {
            if (IsKeyJustPressed(kb, Keys.Escape))
            {
                _gameState = GameState.Paused;
                return;
            }

            ApplyDamageToEntities(dt);
            _playerController.Update(dt, sw, sh);

            AnomalyZone zone = GetPlayerZone();
            _playerController.ProcessAttack(
                mouse, _prevMouse, _camera, zone);

            HandleEnemyAI(dt, sw, sh);
            HandleNpcPickup(kb);
            CheckDeathCondition();
        }

        private bool IsKeyJustPressed(KeyboardState kb, Keys key)
        {
            return kb.IsKeyDown(key)
                && _prevKeyboard.IsKeyUp(key);
        }

        private void ApplyDamageToEntities(float dt)
        {
            Rectangle pRect = GetPlayerRect();
            if (!pRect.Intersects(_damageZone))
            {
                ApplyNpcDamageOnly(dt);
                return;
            }

            _playerModel.TakeDamage(_dps * dt);
            if (_playerModel.State == PlayerState.Carrying)
                _npcModel.TakeDamage(_dps * dt);
            ApplyNpcDamageOnly(dt);
        }

        private void ApplyNpcDamageOnly(float dt)
        {
            if (_npcModel.IsPickedUp) return;
            Rectangle nRect = GetNpcRect();
            if (nRect.Intersects(_damageZone))
                _npcModel.TakeDamage(_dps * dt);
        }

        private Rectangle GetPlayerRect()
        {
            return new Rectangle(
                (int)_playerModel.Position.X,
                (int)_playerModel.Position.Y, 50, 50);
        }

        private Rectangle GetNpcRect()
        {
            return new Rectangle(
                (int)_npcModel.Position.X,
                (int)_npcModel.Position.Y, 40, 40);
        }

        private void HandleEnemyAI(float dt, int sw, int sh)
        {
            if (_enemyModel.IsDead) return;
            _enemyModel.Update(dt);
            _enemyController.Update(
                dt, _playerModel, _npcModel, sw, sh);
        }

        private void HandleNpcPickup(KeyboardState kb)
        {
            if (!IsKeyJustPressed(kb, Keys.E)) return;
            float dist = System.Numerics.Vector2.Distance(
                _playerModel.Position, _npcModel.Position);

            if (_playerModel.State == PlayerState.Free && dist < 70)
                PickupNpc();
            else if (_playerModel.State == PlayerState.Carrying)
                DropNpc();
        }

        private void PickupNpc()
        {
            _npcModel.IsPickedUp = true;
            _playerModel.SetState(PlayerState.Carrying);
        }

        private void DropNpc()
        {
            _npcModel.IsPickedUp = false;
            _npcModel.Position = _playerModel.Position
                + new System.Numerics.Vector2(60, 0);
            _playerModel.SetState(PlayerState.Free);
        }

        private void CheckDeathCondition()
        {
            if (_playerModel.IsDead || _npcModel.IsDead
                || _enemyModel.IsDead)
                _gameState = GameState.GameOver;
        }

        private void HandlePaused(KeyboardState kb,
            MouseState mouse, int sw)
        {
            if (IsKeyJustPressed(kb, Keys.Escape))
            {
                _gameState = GameState.Playing;
                return;
            }

            if (!IsMouseJustPressed(mouse)) return;
            HandlePauseButtons(mouse, sw);
        }

        private void HandlePauseButtons(MouseState mouse, int sw)
        {
            if (_view.GetResumeButton(sw).Contains(mouse.Position))
                _gameState = GameState.Playing;
            if (_view.GetExitButton(sw).Contains(mouse.Position))
                Exit();
        }

        private void HandleGameOver(MouseState mouse, int sw)
        {
            if (!IsMouseJustPressed(mouse)) return;
            if (_view.GetRestartButton(sw).Contains(mouse.Position))
                RestartGame();
        }

        private bool IsMouseJustPressed(MouseState mouse)
        {
            return mouse.LeftButton == ButtonState.Pressed
                && _prevMouse.LeftButton == ButtonState.Released;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SlateGray);
            _spriteBatch.Begin();

            int sw = _graphics.PreferredBackBufferWidth;
            int sh = _graphics.PreferredBackBufferHeight;
            Point mPos = Mouse.GetState().Position;

            DrawWorld();
            DrawMenus(sw, sh, mPos);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            foreach (var zone in _anomalyZones)
                _view.DrawAnomalyZone(_spriteBatch, zone);

            _view.DrawDamageZone(_spriteBatch, _damageZone);
            _view.DrawEnemy(_spriteBatch, _enemyModel);
            _view.DrawNpc(_spriteBatch, _npcModel);
            _view.DrawPlayer(_spriteBatch, _playerModel);
            _view.DrawPlayerAttack(_spriteBatch, _playerModel);
            _view.DrawPlayerUI(_spriteBatch, _playerModel);
        }

        private void DrawMenus(int sw, int sh, Point mPos)
        {
            if (_gameState == GameState.Paused)
                _view.DrawPauseMenu(_spriteBatch, _font, sw, sh, mPos);
            else if (_gameState == GameState.GameOver)
                _view.DrawGameOver(_spriteBatch, _font, sw, sh, mPos);
        }
    }
}

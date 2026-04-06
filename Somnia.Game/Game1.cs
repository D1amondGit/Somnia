using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using Somnia.Game.Views;

namespace Somnia.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyboardState _previousKeyboardState;
        private PlayerModel _playerModel;
        private NpcModel _npcModel;
        private PlayerController _playerController;
        private PlayerView _view;
        private Rectangle _damageZone; 
        private float _damagePerSecond = 20f; 

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _playerModel = new PlayerModel(new System.Numerics.Vector2(200, 200));
            _npcModel = new NpcModel(new System.Numerics.Vector2(600, 400)); 
            _playerController = new PlayerController(_playerModel);
            _damageZone = new Rectangle(400, 300, 200, 200);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new PlayerView(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.Escape)) Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Rectangle playerRect = new Rectangle((int)_playerModel.Position.X, (int)_playerModel.Position.Y, 50, 50);
            if (playerRect.Intersects(_damageZone))
            {
                _playerModel.TakeDamage(_damagePerSecond * deltaTime);
            }
            
            _playerController.Update(deltaTime, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            float distance = System.Numerics.Vector2.Distance(_playerModel.Position, _npcModel.Position);
    
            if (currentKeyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E))
            {
                if (_playerModel.State == PlayerState.Free && distance < 70)
                {
                    _npcModel.IsPickedUp = true;
                    _playerModel.SetState(PlayerState.Carrying);
                }
                else if (_playerModel.State == PlayerState.Carrying)
                {
                    _npcModel.IsPickedUp = false;
                    _npcModel.Position = _playerModel.Position + new System.Numerics.Vector2(60, 0);
                    _playerModel.SetState(PlayerState.Free);
                }
            }

            _previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SlateGray);

            _spriteBatch.Begin();

            // 1. Рисуем зону урона
            _view.DrawDamageZone(_spriteBatch, _damageZone); 

            // 2. РИСУЕМ ПЕРСОНАЖЕЙ (Ты случайно удалил это в прошлый раз)
            _view.DrawNpc(_spriteBatch, _npcModel);
            _view.DrawPlayer(_spriteBatch, _playerModel);
    
            // 3. Рисуем интерфейс
            _view.DrawPlayerUI(_spriteBatch, _playerModel, _graphics.PreferredBackBufferWidth);

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
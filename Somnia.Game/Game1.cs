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
        private Rectangle _damageZone; // Прямоугольник зоны урона
        private float _damagePerSecond = 20f; // Урон в секунду
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // ВКЛЮЧАЕМ ПОЛНЫЙ ЭКРАН
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            
            
        }

        protected override void Initialize()
        {
            _playerModel = new PlayerModel(new System.Numerics.Vector2(200, 200));
            _npcModel = new NpcModel(new System.Numerics.Vector2(600, 400)); // NPC стоит в стороне
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
            // 1. Читаем текущее состояние клавиатуры
            var currentKeyboardState = Keyboard.GetState();
    
            if (currentKeyboardState.IsKeyDown(Keys.Escape)) Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Проверяем, находится ли игрок в зоне урона
            Rectangle playerRect = new Rectangle((int)_playerModel.Position.X, (int)_playerModel.Position.Y, 50, 50);
            if (playerRect.Intersects(_damageZone))
            {
                // Наносим урон игроку
                _playerModel.TakeDamage(_damagePerSecond * deltaTime);
            }
            
            _playerController.Update(deltaTime, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            float distance = System.Numerics.Vector2.Distance(_playerModel.Position, _npcModel.Position);
    
            // 2. ГЛАВНАЯ МАГИЯ ЗДЕСЬ: 
            // Проверяем, что сейчас "E" нажата, а в ПРОШЛОМ кадре была отпущена
            
            
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

            // 3. В самом конце кадра: сохраняем текущее состояние как "прошлое" для следующего кадра
            _previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SlateGray);

            _spriteBatch.Begin();

            // 2. Визуализируем зону урона (полупрозрачный красный)
            _view.DrawDamageZone(_spriteBatch, _damageZone); // Реализуй этот метод во View аналогично DrawNpc

            // ... отрисовка NPC и игрока ...
    
            // 3. Отрисовка UI здоровья (после всех объектов)
            _view.DrawPlayerUI(_spriteBatch, _playerModel, _graphics.PreferredBackBufferWidth);

            _spriteBatch.End();
        }
    }
}
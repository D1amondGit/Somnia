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
        private MouseState _previousMouseState; // НОВОЕ: память мышки
        
        private PlayerModel _playerModel;
        private NpcModel _npcModel;
        private PlayerController _playerController;
        private PlayerView _view;
        private Rectangle _damageZone; 
        private float _damagePerSecond = 10f; 
        private SpriteFont _font;
        private GameState _gameState = GameState.Playing; // НОВОЕ: Текущее состояние игры

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true; // Мышка обязательна для меню!

            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            RestartGame();
            _damageZone = new Rectangle(400, 300, 300, 300); 
            base.Initialize();
        }

        // Метод для сброса игры (вызывается при старте и после смерти)
        private void RestartGame()
        {
            _playerModel = new PlayerModel(new System.Numerics.Vector2(200, 200));
            _npcModel = new NpcModel(new System.Numerics.Vector2(200, 400)); 
            _playerController = new PlayerController(_playerModel);
            _gameState = GameState.Playing;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new PlayerView(GraphicsDevice);
    
            // Загружаем наш скомпилированный шрифт
            _font = Content.Load<SpriteFont>("MainFont");
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int screenWidth = _graphics.PreferredBackBufferWidth;

            // ЕСЛИ МЫ ИГРАЕМ
            if (_gameState == GameState.Playing)
            {
                // Ставим на паузу
                if (currentKeyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                {
                    _gameState = GameState.Paused;
                }
                else
                {
                    // Логика урона
                    Rectangle playerRect = new Rectangle((int)_playerModel.Position.X, (int)_playerModel.Position.Y, 50, 50);
                    if (playerRect.Intersects(_damageZone))
                    {
                        _playerModel.TakeDamage(_damagePerSecond * deltaTime);
                        if (_playerModel.State == PlayerState.Carrying) _npcModel.TakeDamage(_damagePerSecond * deltaTime);
                    }

                    if (!_npcModel.IsPickedUp)
                    {
                        Rectangle npcRect = new Rectangle((int)_npcModel.Position.X, (int)_npcModel.Position.Y, 40, 40);
                        if (npcRect.Intersects(_damageZone)) _npcModel.TakeDamage(_damagePerSecond * deltaTime);
                    }
            
                    // Движение и взаимодействие
                    _playerController.Update(deltaTime, screenWidth, _graphics.PreferredBackBufferHeight);

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

                    // Проверка на смерть
                    if (_playerModel.IsDead || _npcModel.IsDead)
                    {
                        _gameState = GameState.GameOver;
                    }
                }
            }
            // ЕСЛИ МЕНЮ ПАУЗЫ
            else if (_gameState == GameState.Paused)
            {
                // Снимаем с паузы
                if (currentKeyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                {
                    _gameState = GameState.Playing;
                }

                // Клики мышкой
                if (currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_view.GetResumeButton(screenWidth).Contains(currentMouseState.Position)) 
                        _gameState = GameState.Playing; // Зеленая кнопка
                    
                    if (_view.GetExitButton(screenWidth).Contains(currentMouseState.Position)) 
                        Exit(); // Красная кнопка
                }
            }
            // ЕСЛИ МЕНЮ СМЕРТИ
            else if (_gameState == GameState.GameOver)
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_view.GetRestartButton(screenWidth).Contains(currentMouseState.Position)) 
                        RestartGame(); // Зеленая кнопка (Рестарт)
                }
            }

            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState; // Запоминаем мышку
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SlateGray);
            _spriteBatch.Begin();

            int sWidth = _graphics.PreferredBackBufferWidth;
            int sHeight = _graphics.PreferredBackBufferHeight;
            Point mPos = Mouse.GetState().Position;

            // Всегда рисуем мир (он будет под меню)
            _view.DrawDamageZone(_spriteBatch, _damageZone); 
            _view.DrawNpc(_spriteBatch, _npcModel);
            _view.DrawPlayer(_spriteBatch, _playerModel);
            _view.DrawPlayerUI(_spriteBatch, _playerModel);

            // Рисуем меню поверх мира в зависимости от состояния
            // Рисуем меню поверх мира в зависимости от состояния
            if (_gameState == GameState.Paused)
            {
                _view.DrawPauseMenu(_spriteBatch, _font, sWidth, sHeight, mPos);
            }
            else if (_gameState == GameState.GameOver)
            {
                _view.DrawGameOver(_spriteBatch, _font, sWidth, sHeight, mPos);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
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
        private PlayerModel _p;
        private NpcModel _npc;
        private PlayerController _pCtrl;
        private EnemyController _eCtrl;
        private PlayerView _view;
        private List<EnemyModel> _enemies;
        private List<AnomalyZone> _zones;
        private SpriteFont _font;
        private Vector2 _camPos;

        public Game1() 
        { 
            _graphics = new GraphicsDeviceManager(this); 
            _graphics.PreferredBackBufferWidth = 1280; 
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content"; 
            IsMouseVisible = true; 
        }

        protected override void Initialize() { Restart(); base.Initialize(); }
        
        private void Restart() 
        {
            _p = new PlayerModel(new Vector2(400, 300));
            _npc = new NpcModel(new Vector2(600, 400));
            _pCtrl = new PlayerController(_p);
            _eCtrl = new EnemyController();
            
            _enemies = new List<EnemyModel> { new EnemyModel(new Vector2(800, 400)), new EnemyModel(new Vector2(850, 450)) };
            
            _zones = new List<AnomalyZone> {
                new AnomalyZone(new Rectangle(-1000, -2000, 1600, 4000), AnomalyType.Red),
                new AnomalyZone(new Rectangle(600, -2000, 400, 4000), AnomalyType.Neutral),
                new AnomalyZone(new Rectangle(1000, -2000, 2000, 4000), AnomalyType.Blue)
            };
            _camPos = _p.Position - new Vector2(640, 360);
        }

        protected override void LoadContent() 
        { 
            _spriteBatch = new SpriteBatch(GraphicsDevice); 
            _view = new PlayerView(GraphicsDevice);
            try { _font = Content.Load<SpriteFont>("MainFont"); } catch { }
        }

        protected override void Update(GameTime gt) 
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            
            Matrix cam = Matrix.CreateTranslation(new Vector3(-_camPos, 0));
            
            _pCtrl.Update(dt, 3000, 3000, _enemies, cam);
            _eCtrl.Update(dt, _enemies, _p, _npc);
            
            foreach(var e in _enemies) e.Update(dt);
            UpdateZone();
            
            _camPos = Vector2.Lerp(_camPos, _p.Position - new Vector2(640, 360), 0.05f);
            base.Update(gt);
        }

        private void UpdateZone() 
        {
            _p.CurrentZone = AnomalyType.Neutral;
            foreach(var z in _zones) 
                if (z.Area.Contains(_p.Position.X, _p.Position.Y)) _p.CurrentZone = z.Type;
        }

        protected override void Draw(GameTime gt) 
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            Matrix cam = Matrix.CreateTranslation(new Vector3(-_camPos, 0));
            
            _spriteBatch.Begin(transformMatrix: cam);
            _view.DrawWorld(_spriteBatch, _p, _enemies, _zones);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _view.DrawUI(_spriteBatch, _p, _font);
            _spriteBatch.End();

            base.Draw(gt);
        }
    }
}
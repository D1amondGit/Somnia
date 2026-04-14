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
        private GraphicsDeviceManager _gfx;
        private SpriteBatch _sb;
        private PlayerModel _p;
        private NpcModel _npc;
        private PlayerController _pCtrl;
        private EnemyController _eCtrl;
        private PlayerView _view;
        private List<EnemyModel> _enemies;
        private List<AnomalyZone> _zones;
        private SpriteFont _font;
        private KeyboardState _prevK;
        private GameState _state; 

        public Game1() 
        { 
            _gfx = new GraphicsDeviceManager(this); 
            _gfx.PreferredBackBufferWidth = 1280; 
            _gfx.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content"; 
            IsMouseVisible = true; 
        }

        protected override void Initialize() { Restart(); base.Initialize(); }
        
        private void Restart() 
        {
            _p = new PlayerModel(new Vector2(50, 50));
            _npc = new NpcModel(new Vector2(150, 100));
            _pCtrl = new PlayerController(_p);
            _eCtrl = new EnemyController();
            
            _enemies = new List<EnemyModel>();
            
            // 3 МАНЕКЕНА В КАЖДУЮ ЗОНУ (Оранжевые квадраты, 10к ХП)
            _enemies.Add(new EnemyModel(new Vector2(200, 500)) { Health=10000, MaxHealth=10000, IsDummy=true }); // Красная зона
            _enemies.Add(new EnemyModel(new Vector2(600, 500)) { Health=10000, MaxHealth=10000, IsDummy=true }); // Зеленая зона
            _enemies.Add(new EnemyModel(new Vector2(1000, 500)) { Health=10000, MaxHealth=10000, IsDummy=true }); // Синяя зона

            // Куча обычных монстров
            _enemies.Add(new EnemyModel(new Vector2(900, 100)));
            _enemies.Add(new EnemyModel(new Vector2(1000, 200)));
            _enemies.Add(new EnemyModel(new Vector2(1100, 300)));
            _enemies.Add(new EnemyModel(new Vector2(950, 400)));
            _enemies.Add(new EnemyModel(new Vector2(1050, 500)));
            _enemies.Add(new EnemyModel(new Vector2(1150, 600)));
            _enemies.Add(new EnemyModel(new Vector2(850, 650)));
            
            _zones = new List<AnomalyZone> {
                new AnomalyZone(new Rectangle(0, 0, 426, 720), AnomalyType.Red),
                new AnomalyZone(new Rectangle(426, 0, 426, 720), AnomalyType.Neutral),
                new AnomalyZone(new Rectangle(852, 0, 428, 720), AnomalyType.Blue)
            };
            
            _state = GameState.Playing; 
        }

        protected override void LoadContent() 
        { 
            _sb = new SpriteBatch(GraphicsDevice); 
            _view = new PlayerView(GraphicsDevice);
            try { _font = Content.Load<SpriteFont>("MainFont"); } catch { }
        }

        protected override void Update(GameTime gt) 
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape) && _prevK.IsKeyUp(Keys.Escape))
                _state = _state == GameState.Playing ? GameState.Paused : GameState.Playing;

            if (_state == GameState.Paused && ks.IsKeyDown(Keys.R)) Restart();
            if (_state == GameState.Playing) UpdatePlaying(gt);

            _prevK = ks;
            base.Update(gt);
        }

        private void UpdatePlaying(GameTime gt)
        {
            if (_p.IsDead || (_npc != null && _npc.IsDead)) { Restart(); return; }
            
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            
            _pCtrl.Update(dt, 1280, 720, _enemies, Matrix.Identity, _npc);
            _eCtrl.Update(dt, _enemies, _p, _npc);
            
            foreach(var e in _enemies) e.Update(dt);
            
            _p.CurrentZone = AnomalyType.Neutral;
            foreach(var z in _zones) 
                if (z.Area.Contains(_p.Position.X, _p.Position.Y)) _p.CurrentZone = z.Type;
        }

        protected override void Draw(GameTime gt) 
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            _sb.Begin();
            _view.DrawWorld(_sb, _p, _enemies, _zones, _npc);
            _view.DrawUI(_sb, _p, _font, 1280, 720); 
            if (_state == GameState.Paused) _view.DrawPauseMenu(_sb, _font, 1280, 720); 
            _sb.End();
            base.Draw(gt);
        }
    }
}
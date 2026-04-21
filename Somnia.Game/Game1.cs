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
        private List<HexagonModel> _walls; 
        private List<ProjectileModel> _projectiles; 
        
        private SpriteFont _font;
        private Texture2D _floorTex;
        private Texture2D _wallTex; // Текстура стены
        
        private KeyboardState _prevK;
        private GameState _state; 
        private Rectangle _hatch; 
        private Rectangle _playArea;

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
            _playArea = new Rectangle(50, 50, 1180, 620);
            
            _p = new PlayerModel(new Vector2(100, 100)); 
            _npc = new NpcModel(new Vector2(200, 150));  
            
            _pCtrl = new PlayerController(_p);
            _eCtrl = new EnemyController();
            _projectiles = new List<ProjectileModel>();
            _enemies = new List<EnemyModel>();
            
            // --- ОДИН ГЕКСАГОН В ЦЕНТРЕ (X, Y, Радиус, Высота Стенки) ---
            _walls = new List<HexagonModel>();
            _walls.Add(new HexagonModel(new Vector2(640, 360), 100f, 60f)); 
            
            _hatch = new Rectangle(1110, 560, 80, 80);

            _zones = new List<AnomalyZone> {
                new AnomalyZone(new Rectangle(50, 50, 590, 620), AnomalyType.Neutral), 
                new AnomalyZone(new Rectangle(640, 50, 590, 310), AnomalyType.Red),   
                new AnomalyZone(new Rectangle(640, 360, 590, 310), AnomalyType.Blue)   
            };
            
            _state = GameState.Playing; 
        }

        protected override void LoadContent() 
        { 
            _sb = new SpriteBatch(GraphicsDevice); 
            _view = new PlayerView(GraphicsDevice);
            try { _font = Content.Load<SpriteFont>("MainFont"); } catch { }
            try { _floorTex = Content.Load<Texture2D>("floor"); } catch { }
            try { _wallTex = Content.Load<Texture2D>("wall"); } catch { } // Загрузка стены
        }

        protected override void Update(GameTime gt) 
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape) && _prevK.IsKeyUp(Keys.Escape)) _state = _state == GameState.Playing ? GameState.Paused : GameState.Playing;
            if (_state == GameState.Paused && ks.IsKeyDown(Keys.R)) Restart();
            if (_state == GameState.Playing) UpdatePlaying(gt);
            _prevK = ks; base.Update(gt);
        }

        private void UpdatePlaying(GameTime gt)
        {
            if (_p.IsDead || (_npc != null && _npc.IsDead)) { Restart(); return; } 
            if (_hatch.Contains(_p.Position) && _p.State == PlayerState.Carrying) { Restart(); return; }
            
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            
            _pCtrl.Update(dt, _playArea, _enemies, Matrix.Identity, _npc, _walls);
            
            // Заглушка для ИИ, чтобы он не крашился при пустых стенах. 
            // (В EnemyController тоже нужно будет поменять List<Vector3> на List<HexagonModel> позже, пока передаем null)
            _eCtrl.Update(dt, _enemies, _p, _npc, _playArea, null, _projectiles); 

            Rectangle pRect = new Rectangle((int)_p.Position.X, (int)_p.Position.Y, 50, 50);
            for (int i = _projectiles.Count - 1; i >= 0; i--) {
                _projectiles[i].Update(dt);
                Vector2 projCenter = _projectiles[i].Position;
                
                bool hitWall = false; 
                foreach(var w in _walls) if (Vector2.Distance(projCenter, w.Center) < w.Radius * 0.866f) hitWall = true;
                
                if (hitWall || !_playArea.Contains(projCenter)) { _projectiles.RemoveAt(i); continue; }
                if (pRect.Contains(projCenter)) { _p.TakeDamage(_projectiles[i].Damage); _projectiles.RemoveAt(i); }
            }
            
            _p.CurrentZone = AnomalyType.Neutral;
            foreach(var z in _zones) if (z.Area.Contains(_p.Position.X, _p.Position.Y)) _p.CurrentZone = z.Type;
        }

        protected override void Draw(GameTime gt) 
        {
            GraphicsDevice.Clear(Color.DarkSlateGray); 
            // PointWrap ОБЯЗАТЕЛЕН, чтобы текстуры пола и стен зацикливались
            _sb.Begin(samplerState: SamplerState.PointWrap);
            
            if (_floorTex != null)
                _sb.Draw(_floorTex, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, 1280, 720), Color.DarkGray);

            // Передаем _wallTex в DrawWorld
            _view.DrawWorld(_sb, _p, _enemies, _zones, _npc, _hatch, _playArea, _walls, _wallTex, _projectiles);
            _view.DrawUI(_sb, _p, _font, 1280, 720, 1); 
            if (_state == GameState.Paused) _view.DrawPauseMenu(_sb, _font, 1280, 720); 
            _sb.End();
            base.Draw(gt);
        }
    }
}
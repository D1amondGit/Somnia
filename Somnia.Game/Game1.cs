using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using Somnia.Game.Views;
using System.Collections.Generic;
using System;
using System.Linq;

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
        private Texture2D _wallTex;
        private KeyboardState _prevK;
        private GameState _state; 
        private Rectangle _hatch; 
        private Rectangle _playArea;
        private Random _rnd = new Random();

        public Game1() 
        { 
            _gfx = new GraphicsDeviceManager(this); 
            _gfx.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _gfx.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _gfx.IsFullScreen = true;
            _gfx.ApplyChanges();
            Content.RootDirectory = "Content"; 
            IsMouseVisible = true; 
        }

        protected override void Initialize() { Restart(); base.Initialize(); }
        
        private void Restart() 
        {
            int w = _gfx.PreferredBackBufferWidth;
            int h = _gfx.PreferredBackBufferHeight;
            // Зона ходьбы (чуть шире, чтобы можно было подходить к стенам)
            _playArea = new Rectangle(80, 80, w - 160, h - 160);
            
            _p = new PlayerModel(new Vector2(w / 2, h / 2)); 
            _npc = new NpcModel(new Vector2(w / 2 + 100, h / 2));  
            
            _pCtrl = new PlayerController(_p);
            _eCtrl = new EnemyController();
            _projectiles = new List<ProjectileModel>();
            _enemies = new List<EnemyModel>();
            _walls = new List<HexagonModel>();

            GenerateBorders(w, h);
            SetupZones(w, h);

            _hatch = new Rectangle(w - 300, h - 300, 100, 100);
            _state = GameState.Playing; 
        }

        private void GenerateBorders(int w, int h)
        {
            float step = 170f; // Шаг между гексагонами
            float r = 130f;    // Радиус

            for (int layer = 0; layer < 2; layer++) 
            {
                float d = layer * 70f; 
                for (float x = -step; x <= w + step; x += step) {
                    // ВЕРХ: Сместил Y в диапазон (20, 70), чтобы они "выглядывали" из-за края
                    AddWall(new Vector2(x + _rnd.Next(-50, 50), _rnd.Next(20, 70) - d), r + _rnd.Next(-20, 20));
                    // НИЗ: Сместил Y в диапазон (h-70, h-20)
                    AddWall(new Vector2(x + _rnd.Next(-50, 50), h - _rnd.Next(20, 70) + d), r + _rnd.Next(-20, 20));
                }
                for (float y = -step; y <= h + step; y += step) {
                    // ЛЕВО И ПРАВО: Сдвинул ближе к центру
                    AddWall(new Vector2(_rnd.Next(20, 70) - d, y + _rnd.Next(-50, 50)), r + _rnd.Next(-20, 20));
                    AddWall(new Vector2(w - _rnd.Next(20, 70) + d, y + _rnd.Next(-50, 50)), r + _rnd.Next(-20, 20));
                }
            }
        }

        private void SpawnArenaObstacles(int w, int h)
        {
            AddWall(new Vector2(w * 0.3f, h * 0.4f), 80f);
            AddWall(new Vector2(w * 0.7f, h * 0.6f), 90f);
        }

        // РЕДАКТИРОВАТЬ ГЕКСАГОН ЗДЕСЬ:
        private void AddWall(Vector2 pos, float r = 100f)
        {
            // (Позиция, Радиус, Высота, ПРИПЛЮСНУТОСТЬ, НАКЛОН)
            // Поставил твои параметры из скрипта: 0.4f (Squash) и 0.04f (Tilt)
            _walls.Add(new HexagonModel(pos, r, 60f, 0.4f, 0.04f));
        }

        private void SetupZones(int w, int h)
        {
            _zones = new List<AnomalyZone>();
            _zones.Add(new AnomalyZone(new Rectangle(0, 0, w / 2, h), AnomalyType.Neutral));
            _zones.Add(new AnomalyZone(new Rectangle(w / 2, 0, w / 2, h / 2), AnomalyType.Red));
            _zones.Add(new AnomalyZone(new Rectangle(w / 2, h / 2, w / 2, h / 2), AnomalyType.Blue));
        }

        protected override void LoadContent() 
        { 
            _sb = new SpriteBatch(GraphicsDevice); 
            _view = new PlayerView(GraphicsDevice);
            try { _font = Content.Load<SpriteFont>("MainFont"); } catch { }
            try { _floorTex = Content.Load<Texture2D>("floor"); } catch { }
            try { _wallTex = Content.Load<Texture2D>("wall"); } catch { }
        }

        protected override void Update(GameTime gt) 
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape)) Exit(); 
            if (_state == GameState.Playing) UpdatePlaying(gt);
            _prevK = ks; base.Update(gt);
        }

        private void UpdatePlaying(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            // ФИКС: Объявляем w и h для этого метода
            int w = _gfx.PreferredBackBufferWidth;
            int h = _gfx.PreferredBackBufferHeight;

            _pCtrl.Update(dt, _playArea, _enemies, Matrix.Identity, _npc, _walls);
            
            for (int i = _projectiles.Count - 1; i >= 0; i--) {
                var pr = _projectiles.ElementAt(i); pr.Update(dt);
                Vector2 pPos = pr.Position;
                // Теперь ошибки нет
                if (pPos.X < 0 || pPos.X > w || pPos.Y < 0 || pPos.Y > h) { _projectiles.RemoveAt(i); continue; }
                
                bool hit = false;
                foreach(var wall in _walls) if (Vector2.Distance(pPos, wall.Center) < wall.Radius * 0.6f) hit = true;
                if (hit) _projectiles.RemoveAt(i);
            }

            if (_hatch.Contains(_p.Position.X, _p.Position.Y) && _p.State == PlayerState.Carrying) Restart();
        }

        protected override void Draw(GameTime gt) 
        {
            GraphicsDevice.Clear(Color.Black); // Фон теперь ВСЕГДА черный
            _sb.Begin(samplerState: SamplerState.PointWrap);
            
            int w = _gfx.PreferredBackBufferWidth;
            int h = _gfx.PreferredBackBufferHeight;

            if (_floorTex != null)
                _sb.Draw(_floorTex, new Rectangle(0, 0, w, h), new Rectangle(0, 0, w, h), Color.DarkGray);

            _view.DrawWorld(_sb, _p, _enemies, _zones, _npc, _hatch, _playArea, _walls, _wallTex, _projectiles);
            _view.DrawUI(_sb, _p, _font, w, h, 1); 
            _sb.End();
            base.Draw(gt);
        }
    }
}
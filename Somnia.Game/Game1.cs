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
        private List<Vector3> _walls; 
        private List<ProjectileModel> _projectiles; 
        
        private SpriteFont _font;
        private Texture2D _floorTex;
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
            // Отступы по 50 пикселей со всех сторон (для жирной черной рамки)
            _playArea = new Rectangle(50, 50, 1180, 620);
            
            // Спавны из скетча (левый верхний угол)
            _p = new PlayerModel(new Vector2(100, 100)); // Зеленая точка
            _npc = new NpcModel(new Vector2(200, 150));  // Желтая точка
            
            _pCtrl = new PlayerController(_p);
            _eCtrl = new EnemyController();
            _projectiles = new List<ProjectileModel>();
            _enemies = new List<EnemyModel>();
            _walls = new List<Vector3>();
            
            // СТЕНЫ ИЗ СКЕТЧА (X, Y, Радиус)
            _walls.Add(new Vector3(300, 500, 70));  // Нижний левый круг (В Зеленой зоне)
            _walls.Add(new Vector3(640, 200, 80));  // Верхний центральный круг (Между Зеленой и Красной)
            _walls.Add(new Vector3(1000, 360, 70)); // Правый центральный круг (Между Красной и Синей)
            
            // ЛЮК ИЗ СКЕТЧА (Синяя точка в правом нижнем углу)
            _hatch = new Rectangle(1110, 560, 80, 80);

            // ВРАГИ ИЗ СКЕТЧА (Красные точки)
            _enemies.Add(new EnemyModel(new Vector2(500, 360), EnemyType.Melee)); // Центр
            _enemies.Add(new EnemyModel(new Vector2(900, 150), EnemyType.Melee)); // Верхний правый
            _enemies.Add(new EnemyModel(new Vector2(800, 550), EnemyType.Melee)); // Нижний правый
            
            // Дальник в обводке
            _enemies.Add(new EnemyModel(new Vector2(1100, 100), EnemyType.Shooter)); 
            
            // ЗОНЫ ИЗ СКЕТЧА (Зеленая половина, Красная и Синяя четвертинки)
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
            _eCtrl.Update(dt, _enemies, _p, _npc, _playArea, _walls, _projectiles);
            
            for (int i = _enemies.Count - 1; i >= 0; i--) {
                _enemies[i].Update(dt);
                
                Vector2 center = _enemies[i].Position + new Vector2(20, 20);
                foreach (var w in _walls) {
                    Vector2 wCenter = new Vector2(w.X, w.Y);
                    float dist = Vector2.Distance(center, wCenter);
                    float minDist = 20f + w.Z;
                    if (dist < minDist && dist > 0) {
                        _enemies[i].Position += Vector2.Normalize(center - wCenter) * (minDist - dist);
                        center = _enemies[i].Position + new Vector2(20, 20);
                    }
                }

                _enemies[i].Position = new Vector2(
                    MathHelper.Clamp(_enemies[i].Position.X, _playArea.X, _playArea.Right - 40),
                    MathHelper.Clamp(_enemies[i].Position.Y, _playArea.Y, _playArea.Bottom - 40)
                );

                if (_enemies[i].IsDead) _enemies.RemoveAt(i);
            }

            Rectangle pRect = new Rectangle((int)_p.Position.X, (int)_p.Position.Y, 50, 50);
            for (int i = _projectiles.Count - 1; i >= 0; i--) {
                _projectiles[i].Update(dt);
                Vector2 projCenter = _projectiles[i].Position;
                
                bool hitWall = false; 
                foreach(var w in _walls) if (Vector2.Distance(projCenter, new Vector2(w.X, w.Y)) < w.Z + 5f) hitWall = true;
                
                if (hitWall || !_playArea.Contains(projCenter)) { _projectiles.RemoveAt(i); continue; }
                if (pRect.Contains(projCenter)) { _p.TakeDamage(_projectiles[i].Damage); _projectiles.RemoveAt(i); }
            }
            
            _p.CurrentZone = AnomalyType.Neutral;
            foreach(var z in _zones) if (z.Area.Contains(_p.Position.X, _p.Position.Y)) _p.CurrentZone = z.Type;
        }

        protected override void Draw(GameTime gt) 
        {
            GraphicsDevice.Clear(Color.DarkSlateGray); 
            _sb.Begin(samplerState: SamplerState.PointWrap);
            
            if (_floorTex != null)
                _sb.Draw(_floorTex, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, 1280, 720), Color.DarkGray);

            _view.DrawWorld(_sb, _p, _enemies, _zones, _npc, _hatch, _playArea, _walls, _projectiles);
            _view.DrawUI(_sb, _p, _font, 1280, 720, 1); 
            if (_state == GameState.Paused) _view.DrawPauseMenu(_sb, _font, 1280, 720); 
            _sb.End();
            base.Draw(gt);
        }
    }
}
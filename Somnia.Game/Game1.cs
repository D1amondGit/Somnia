using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using Somnia.Game.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Somnia.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _gfx;
        private SpriteBatch _sb;
        private PlayerModel _p;
        private PlayerController _pCtrl;
        private PlayerView _view;
        private List<EnemyModel> _enemies;
        private EnemyController _eCtrl; 
        private List<AnomalyZone> _zones;
        private NpcModel _npc;
        private WaveManager _waveManager;
        private List<ResourceDropModel> _drops;
        private List<FloatingText> _texts; 
        private List<GateModel> _gates;
        private List<HexagonModel> _walls; 
        private List<ProjectileModel> _projectiles;
        private Texture2D _wallTex, _floorTex;
        private SpriteFont _font;
        private KeyboardState _prevKs;
        private Rectangle _playArea;
        private int _state = 0;
        private Random _rnd = new Random();

        public Game1()
        {
            _gfx = new GraphicsDeviceManager(this);
            _gfx.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _gfx.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _gfx.IsFullScreen = true; _gfx.ApplyChanges();
            Content.RootDirectory = "Content"; IsMouseVisible = true;
        }

        protected override void Initialize() { Restart(); base.Initialize(); }

        private void Restart()
        {
            _state = 0;
            _playArea = new Rectangle(0, 0, _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight); 
            _p = new PlayerModel(new Vector2(250, _playArea.Height / 2));
            _npc = new NpcModel(new Vector2(250, _playArea.Height / 2 + 50));
            _pCtrl = new PlayerController(_p); _eCtrl = new EnemyController();
            _waveManager = new WaveManager(); _projectiles = new List<ProjectileModel>();
            _drops = new List<ResourceDropModel>(); _texts = new List<FloatingText>();
            _gates = new List<GateModel> { new GateModel(new Vector2(_playArea.Width - 200, _playArea.Height / 2)) };
            
            _zones = CreateRandomZones(_playArea.Width, _playArea.Height);
            GenerateBorders(_playArea.Width, _playArea.Height); GenerateObstacles(_playArea.Width, _playArea.Height);
            _enemies = _waveManager.SpawnCurrentWave(_playArea.Width, _playArea.Height);
        }

        private void NextLevel()
        {
            _waveManager.AdvanceArena();
            if (_waveManager.AllArenasCleared) { _state = 2; return; } 
    
            _p.Position = new Vector2(250, _playArea.Height / 2); 
            _p.SetState(PlayerState.Free);
            _npc.Position = new Vector2(250, _playArea.Height / 2 + 50); 
            _npc.IsPickedUp = false;
    
            _projectiles.Clear(); 
            _drops.Clear(); 
            _texts.Clear();

            // Исправленная часть:
            _gates.Clear();
            _gates.Add(new GateModel(new Vector2(_playArea.Width - 200, _playArea.Height / 2)));

            _zones = CreateRandomZones(_playArea.Width, _playArea.Height);
            _walls.Clear(); 
            GenerateBorders(_playArea.Width, _playArea.Height); 
            GenerateObstacles(_playArea.Width, _playArea.Height);
            _enemies = _waveManager.SpawnCurrentWave(_playArea.Width, _playArea.Height);
        }

        private List<AnomalyZone> CreateRandomZones(int w, int h)
        {
            var zList = new List<AnomalyZone>();
            var tList = new List<AnomalyType> { AnomalyType.Red, AnomalyType.Blue, AnomalyType.Green };
            int tries = 0;
            while (zList.Count < 6 && tries < 200) {
                tries++; float r = _rnd.Next(150, 350);
                Vector2 p = new Vector2(_rnd.Next((int)r, w - (int)r), _rnd.Next((int)r, h - (int)r));
                bool valid = true;
                foreach (var z in zList) if (Vector2.Distance(p, z.Center) < r + z.Radius + 30f) valid = false;
                if (valid) zList.Add(new AnomalyZone(p, r, tList.ElementAt(_rnd.Next(3))));
            } return zList;
        }

        private void GenerateBorders(int w, int h)
        {
            _walls = new List<HexagonModel>(); float step = 140f; float r = 120f;
            for (float x = -step; x <= w + step; x += step) {
                _walls.Add(new HexagonModel(new Vector2(x, -50), r, 70f, 0.7f, 0.04f));
                _walls.Add(new HexagonModel(new Vector2(x, h + 50), r, 70f, 0.7f, 0.04f));
            }
            for (float y = -step; y <= h + step; y += step) {
                _walls.Add(new HexagonModel(new Vector2(-50, y), r, 70f, 0.7f, 0.04f));
                _walls.Add(new HexagonModel(new Vector2(w + 50, y), r, 70f, 0.7f, 0.04f));
            }
        }

        private void GenerateObstacles(int w, int h)
        {
            for (int i = 0; i < 15; i++) {
                float rx = _rnd.Next(400, w - 400); float ry = _rnd.Next(150, h - 150);
                _walls.Add(new HexagonModel(new Vector2(rx, ry), _rnd.Next(40, 90), _rnd.Next(40, 90), 0.7f, 0.04f));
            }
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice); _view = new PlayerView(GraphicsDevice);
            try { _font = Content.Load<SpriteFont>("MainFont"); } catch { }
            try { _wallTex = Content.Load<Texture2D>("wall"); } catch { } 
            try { _floorTex = Content.Load<Texture2D>("floor"); } catch { }
        }

        protected override void Update(GameTime gt)
        {
            var ks = Keyboard.GetState();
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.Escape) && _prevKs.IsKeyUp(Keys.Escape)) _state = _state == 0 ? 1 : 0;
            if (_p.IsDead || _npc.IsDead) _state = 2; 
            if ((_state == 2 || _state == 1) && ks.IsKeyDown(Keys.Enter)) Restart();

            if (_state == 0) RunGameLogic(dt, ks);
            _prevKs = ks; base.Update(gt);
        }
        private void RunGameLogic(float dt, KeyboardState ks)
        {
            _p.Update(dt); _p.UpdateSkills(dt, _enemies); 
            _pCtrl.Update(dt, _playArea.Width, _playArea.Height, _enemies, Matrix.Identity, _npc, _walls);
            
            // Физика Игрока
            foreach (var w in _walls) PhysicsHelper.ResolveHexCollision(ref _p.Position, 25f, w);
            
            HandleNpcPickup(ks);
            if (_npc.IsPickedUp) _npc.Position = _p.Position + new Vector2(35, -20);
            
            UpdateEnemies(dt); UpdateProjectiles(dt); ProcessDeadEnemies(); UpdateDrops(dt);
            
            foreach (var g in _gates) {
                g.TryOpen(_p, _npc);
                if (g.IsOpen) { NextLevel(); return; }
            }
            
            _p.CurrentZone = AnomalyType.Neutral;
            foreach (var z in _zones) if (z.ContainsPoint(_p.Position)) _p.CurrentZone = z.Type;
            _p.DamageMultiplier = _npc.IsInjured ? 0.5f : 1.0f;
        }

        private void UpdateEnemies(float dt)
        {
            var w3 = _walls.Select(w => new Vector3(w.Center.X, w.Center.Y, w.Radius)).ToList();
            foreach (var e in _enemies) e.Update(dt);
            
            // Сначала умное движение (Steering)
            _eCtrl.Update(dt, _enemies, _p, _npc, _playArea, w3, _projectiles);
            
            // Затем жесткое выталкивание (Collision Resolution)
            foreach (var e in _enemies.Where(x => !x.IsDead)) {
                foreach (var w in _walls) PhysicsHelper.ResolveHexCollision(ref e.Position, 20f, w);
            }
        }
        

        private void HandleNpcPickup(KeyboardState ks)
        {
            if (_prevKs.IsKeyDown(Keys.E) || !ks.IsKeyDown(Keys.E)) return;
            if (_p.State == PlayerState.Free && Vector2.Distance(_p.Position, _npc.Position) < 80f) {
                _p.SetState(PlayerState.Carrying); _npc.IsPickedUp = true;
            } else if (_p.State == PlayerState.Carrying) {
                _p.SetState(PlayerState.Free); _npc.IsPickedUp = false;
            }
        }

       

        private void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--) {
                var pr = _projectiles.ElementAt(i);
                pr.Position += pr.Velocity * dt; pr.LifeTime -= dt;
                if (pr.LifeTime <= 0) { _projectiles.RemoveAt(i); continue; }
                
                if (Vector2.Distance(pr.Position, _p.Position) < 30f) { _p.TakeDamage(10f); _projectiles.RemoveAt(i); }
                else if (!_npc.IsPickedUp && Vector2.Distance(pr.Position, _npc.Position) < 30f) { _npc.TakeDamage(10f); _projectiles.RemoveAt(i); }
            }
        }

        private void ProcessDeadEnemies()
        {
            foreach (var e in _enemies) {
                if (!e.IsDead || e.HasDropped) continue;
                e.HasDropped = true;
                _drops.Add(new ResourceDropModel(e.Position + new Vector2(-15, 15), DropType.Health, 15f));
                _drops.Add(new ResourceDropModel(e.Position + new Vector2(15, -15), DropType.Mana, 10f));
            }
        }

        private void UpdateDrops(float dt)
        {
            foreach (var d in _drops) d.Update(_p.Position);
            foreach (var d in _drops) {
                if (!d.Collected) continue;
                if (d.Type == DropType.Health) { _p.CurrentHealth = MathHelper.Min(_p.MaxHealth, _p.CurrentHealth + d.Value); _texts.Add(new FloatingText { Position = _p.Position, Text = "+HP", Color = Color.Lime }); }
                if (d.Type == DropType.Mana) { _p.CurrentMana = MathHelper.Min(100f, _p.CurrentMana + d.Value); _texts.Add(new FloatingText { Position = _p.Position, Text = "+MP", Color = Color.Cyan }); }
            }
            _drops.RemoveAll(d => d.Collected);
            foreach (var t in _texts) { t.Position.Y -= 60f * dt; t.Timer -= dt; }
            _texts.RemoveAll(t => t.Timer <= 0);
        }

        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            _sb.Begin(samplerState: SamplerState.PointWrap);
            if (_floorTex != null) _sb.Draw(_floorTex, _playArea, _playArea, Color.DarkGray);
            _view.DrawWorld(_sb, _p, _enemies, _zones, _npc, _playArea, _walls, _wallTex, _drops, _gates, _texts, _font, _projectiles);
            _sb.End();

            _sb.Begin();
            _view.DrawUI(_sb, _p, _npc, _font, _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight, _state, _waveManager.CurrentArena + 1);
            _sb.End();
        }
    }
}
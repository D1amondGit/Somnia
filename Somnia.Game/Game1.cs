using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using Somnia.Game.Views;
using System;
using System.Collections.Generic;

namespace Somnia.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlayerModel _p;
        private PlayerController _pCtrl;
        private PlayerView _view;
        private List<EnemyModel> _enemies;
        private List<EnemyController> _enemyControllers;
        private List<AnomalyZone> _zones;
        private NpcModel _npc;
        private WaveManager _waveManager;
        private List<ResourceDropModel> _drops;
        private List<GateModel> _gates;
        private SpriteFont _font;
        private Vector2 _camPos;
        private KeyboardState _prevKs;

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
            _p = new PlayerModel(new System.Numerics.Vector2(400, 400));
            _pCtrl = new PlayerController(_p);
            _npc = new NpcModel(new System.Numerics.Vector2(350, 400));
            _waveManager = new WaveManager();
            _drops = new List<ResourceDropModel>();
            _zones = CreateZones();
            _gates = new List<GateModel> { new GateModel(new System.Numerics.Vector2(3200, 400)) };
            SpawnNextWave();
        }

        private List<AnomalyZone> CreateZones()
        {
            return new List<AnomalyZone>
            {
                new AnomalyZone(new Rectangle(0, 0, 1000, 1200), AnomalyType.Red),
                new AnomalyZone(new Rectangle(1000, 150, 600, 500), AnomalyType.Neutral),
                new AnomalyZone(new Rectangle(1600, 0, 2000, 1200), AnomalyType.Blue)
            };
        }

        private void SpawnNextWave()
        {
            _enemies = _waveManager.SpawnCurrentWave();
            _enemyControllers = _enemies.ConvertAll(e => new EnemyController(e));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _view = new PlayerView(GraphicsDevice);
            _font = Content.Load<SpriteFont>("MainFont");
        }

        protected override void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            Matrix cam = Matrix.CreateTranslation(new Vector3(-_camPos, 0));

            _pCtrl.Update(dt, 4000, 1200, _enemies, cam);
            HandleNpcPickup(ks);
            UpdateNpc();
            UpdateEnemies(dt);
            ProcessDeadEnemies();
            UpdateDrops();
            UpdateGates();
            UpdateZone();
            UpdateDamageMultiplier();
            _waveManager.CheckWaveCleared(_enemies);
            if (_waveManager.WaveJustCleared && !_waveManager.AllArenasCleared)
                SpawnNextWave();

            _camPos = Vector2.Lerp(_camPos, new Vector2(_p.Position.X - 640, _p.Position.Y - 360), 0.1f);
            _prevKs = ks;
            base.Update(gt);
        }

        private void HandleNpcPickup(KeyboardState ks)
        {
            if (_prevKs.IsKeyDown(Keys.E) || !ks.IsKeyDown(Keys.E)) return;
            if (_p.State == PlayerState.Free && System.Numerics.Vector2.Distance(_p.Position, _npc.Position) < 80f)
            {
                _p.SetState(PlayerState.Carrying);
                _npc.IsPickedUp = true;
            }
            else if (_p.State == PlayerState.Carrying)
            {
                _p.SetState(PlayerState.Free);
                _npc.IsPickedUp = false;
            }
        }

        private void UpdateNpc()
        {
            if (_npc.IsPickedUp)
                _npc.Position = _p.Position + new System.Numerics.Vector2(35, -20);
        }

        private void UpdateEnemies(float dt)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].Update(dt);
                _enemyControllers[i].Update(dt, _p, _npc, 4000, 1200);
            }
        }

        private void ProcessDeadEnemies()
        {
            foreach (var e in _enemies)
            {
                if (!e.IsDead || e.HasDropped) continue;
                e.HasDropped = true;
                _drops.Add(new ResourceDropModel(e.Position + new System.Numerics.Vector2(-10, 10), DropType.Health, 15f));
                _drops.Add(new ResourceDropModel(e.Position + new System.Numerics.Vector2(10, -10), DropType.Mana, 10f));
            }
        }

        private void UpdateDrops()
        {
            foreach (var drop in _drops)
                drop.Update(_p.Position);

            foreach (var drop in _drops)
            {
                if (!drop.Collected || drop.Type != DropType.Health) continue;
                _p.CurrentHealth = Math.Min(_p.MaxHealth, _p.CurrentHealth + drop.Value);
            }
            _drops.RemoveAll(d => d.Collected);
        }

        private void UpdateGates()
        {
            foreach (var gate in _gates)
                gate.TryOpen(_p, _npc);
        }

        private void UpdateZone()
        {
            _p.CurrentZone = AnomalyType.Neutral;
            foreach (var z in _zones)
                if (z.ContainsPoint(_p.Position)) _p.CurrentZone = z.Type;
        }

        private void UpdateDamageMultiplier()
        {
            _p.DamageMultiplier = _npc.IsInjured ? 0.5f : 1.0f;
        }

        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            Matrix cam = Matrix.CreateTranslation(new Vector3(-_camPos, 0));

            _spriteBatch.Begin(transformMatrix: cam);
            _view.DrawWorld(_spriteBatch, _p, _enemies, _zones, _npc, _drops, _gates);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _view.DrawUI(_spriteBatch, _p, _npc, _font, GraphicsDevice.Viewport.Width);
            _spriteBatch.End();
        }
    }
}

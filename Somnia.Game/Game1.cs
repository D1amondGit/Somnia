using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Session;
using Somnia.Game.Views;

namespace Somnia.Game;

/// <summary>Точка входа MonoGame: загрузка контента и делегирование кадров оркестратору.</summary>
public class Game1 : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _gfx;
    private SpriteBatch _spriteBatch = null!;
    private readonly GameplaySessionState _session = new();
    private GameplayOrchestrator _orchestrator = null!;
    private WorldSceneView _worldScene = null!;
    private HudView _hudView = null!;
    private SpriteFont? _font;
    private Texture2D? _wallTex;
    private KeyboardState _prevKeyboard;
    private readonly Random _random = new();

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

    protected override void Initialize()
    {
        _session.Player = new PlayerModel(Vector2.Zero);
        _session.Npc = new NpcModel(Vector2.Zero);
        _orchestrator = new GameplayOrchestrator(_session.Player);
        _orchestrator.RestartGame(_session, _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight, _random);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldScene = new WorldSceneView(GraphicsDevice);
        _hudView = new HudView(GraphicsDevice);

        try
        {
            _font = Content.Load<SpriteFont>("MainFont");
        }
        catch
        {
            _font = null;
        }

        try
        {
            _wallTex = Content.Load<Texture2D>("wall");
        }
        catch
        {
            _wallTex = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboard.IsKeyDown(Keys.Escape) && _prevKeyboard.IsKeyUp(Keys.Escape))
            _session.UiState = _session.UiState == 0 ? 1 : 0;

        if (_session.Player.IsDead || _session.Npc.IsDead)
            _session.UiState = 2;

        if ((_session.UiState == 2 || _session.UiState == 1) && keyboard.IsKeyDown(Keys.Enter))
            _orchestrator.RestartGame(_session, _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight,
                _random);

        if (_session.UiState == 0)
            _orchestrator.SimulatePlayingFrame(_session, dt, keyboard, _prevKeyboard, Matrix.Identity);

        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 12, 16));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _worldScene.Draw(
            _spriteBatch,
            _session.PlayArea,
            _session.ArenaLayoutSeed,
            _session.Player,
            _session.Enemies,
            _session.Zones,
            _session.Npc,
            _session.Walls,
            _wallTex,
            _session.Drops,
            _session.Gates,
            _session.FloatingTexts,
            _font,
            _session.EnemyProjectiles,
            _session.PlayerProjectiles);

        _spriteBatch.End();

        _spriteBatch.Begin();
        _hudView.Draw(
            _spriteBatch,
            _session.Player,
            _session.Npc,
            _font,
            _gfx.PreferredBackBufferWidth,
            _gfx.PreferredBackBufferHeight,
            _session.UiState,
            _session.Waves.CurrentArena + 1,
            gameTime.TotalGameTime.TotalSeconds);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Controllers;
using Somnia.Game.Models;
using Somnia.Game.Services.Audio;
using Somnia.Game.Services.World;
using Somnia.Game.Session;
using Somnia.Game.Views;

namespace Somnia.Game;

/// <summary>Точка входа MonoGame. Тонкая, делегирует кадры оркестратору и видам.</summary>
public class Game1 : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _gfx;
    private SpriteBatch _spriteBatch = null!;
    private readonly GameplaySessionState _session = new();
    private GameplayOrchestrator _orchestrator = null!;
    private WorldSceneView _worldScene = null!;
    private HudView _hudView = null!;
    private MenuView _menuView = null!;
    private SettingsView _settingsView = null!;
    private MenuController _menuController = null!;
    private SettingsController _settingsController = null!;
    private readonly SettingsState _settingsState = new();
    private int _phaseBeforeSettings;
    private AudioController _audio = null!;
    private SkillIconAtlas _iconAtlas = null!;
    private SpriteFont? _font;
    private Texture2D? _wallTex;
    /// <summary>Опционально из Content/floor.png.</summary>
    private Texture2D? _floorTexContent;
    /// <summary>Процедурная текстура пола на текущую арену.</summary>
    private Texture2D? _floorTexProcedural;
    private int _floorBuiltForArenaSeed = int.MinValue;
    private int _floorSettingsFingerprint = int.MinValue;
    private KeyboardState _prevKeyboard;
    private readonly Random _random = new();
    private bool _combatTrackOn;
    private EntityCharacterSprites _entitySprites = null!;
    private Vector2 _playerFrameDisp;
    private Vector2 _npcFrameDisp;

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
        _session.Waves = new Services.Waves.WaveManager();
        _orchestrator = new GameplayOrchestrator(_session.Player);
        _menuController = new MenuController();
        _settingsController = new SettingsController();
        _audio = new AudioController();
        _orchestrator.SetAudio(_audio);

        _session.PlayArea = new Rectangle(0, 0, _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight);
        _session.UiState = GameplayPhase.Title;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldScene = new WorldSceneView(GraphicsDevice);
        _hudView = new HudView(GraphicsDevice);
        _menuView = new MenuView(GraphicsDevice);
        _settingsView = new SettingsView(GraphicsDevice);
        _iconAtlas = new SkillIconAtlas();
        _entitySprites = new EntityCharacterSprites();

        try { _font = Content.Load<SpriteFont>("MainFont"); } catch { _font = null; }
        try { _wallTex = Content.Load<Texture2D>("wall"); } catch { _wallTex = null; }
        try { _floorTexContent = Content.Load<Texture2D>("floor"); } catch { _floorTexContent = null; }

        _audio.LoadContent(Content);
        _audio.PlayMenuTrack();

        _iconAtlas.LoadContent(Content);
        _entitySprites.LoadContent(Content);
        _hudView.UseIconAtlas(_iconAtlas);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_session.UiState == GameplayPhase.Playing
            && (_session.Player.IsDead || _session.Npc.IsDead))
        {
            _session.UiState = GameplayPhase.GameOver;
            _audio.PlayMenuTrack();
            _combatTrackOn = false;
        }

        HandleMenuInput(keyboard);
        ToggleCombatTrack();

        if (_session.UiState == GameplayPhase.Playing)
        {
            RefreshProceduralFloorIfNeeded();

            // Скрытый шорткат на босс-арену: Ctrl + Shift + B (один раз по нажатию B).
            if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.LeftShift) &&
                keyboard.IsKeyDown(Keys.B) && _prevKeyboard.IsKeyUp(Keys.B))
                _orchestrator.DebugJumpToBossArena(_session, _random);

            // Секретная мясорубка: Ctrl + Shift + M.
            if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.LeftShift) &&
                keyboard.IsKeyDown(Keys.M) && _prevKeyboard.IsKeyUp(Keys.M))
                _orchestrator.DebugEnterSecretMeatGrinder(_session, _random);

            try
            {
                var p0 = _session.Player.Position;
                var n0 = _session.Npc.Position;
                _orchestrator.SimulatePlayingFrame(_session, dt, keyboard, _prevKeyboard,
                    _session.Camera.InputTransform);
                _playerFrameDisp = _session.Player.Position - p0;
                _npcFrameDisp = _session.Npc.Position - n0;
            }
            catch (System.Exception ex)
            {
                // Запишем в консоль/Output, но не валим всю игру.
                System.Console.Error.WriteLine($"[Somnia] frame error: {ex}");
                System.Diagnostics.Debug.WriteLine($"[Somnia] frame error: {ex}");
            }
        }

        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    private void HandleMenuInput(KeyboardState keyboard)
    {
        // Экран настроек — отдельный обработчик.
        if (_session.UiState == GameplayPhase.Settings)
        {
            var settingsCmd = _settingsController.Update(keyboard, _settingsState, _audio);
            if (settingsCmd == SettingsController.SettingsCommand.Back)
                _session.UiState = _phaseBeforeSettings;
            return;
        }

        // Пауза по ESC во время игры
        if (_session.UiState == GameplayPhase.Playing
            && keyboard.IsKeyDown(Keys.Escape) && _prevKeyboard.IsKeyUp(Keys.Escape))
        {
            _session.UiState = GameplayPhase.Paused;
            return;
        }

        var cmd = _menuController.Update(_prevKeyboard, keyboard, _session.UiState);
        switch (cmd)
        {
            case MenuCommand.StartNewRun:
            case MenuCommand.RestartRun:
                _orchestrator.RestartGame(_session,
                    _gfx.PreferredBackBufferWidth,
                    _gfx.PreferredBackBufferHeight,
                    _random);
                break;
            case MenuCommand.Resume:
                _session.UiState = GameplayPhase.Playing;
                break;
            case MenuCommand.OpenSettings:
                _phaseBeforeSettings = _session.UiState;
                _session.UiState = GameplayPhase.Settings;
                break;
            case MenuCommand.ReturnToTitle:
                _session.SecretMeatVictory = false;
                _session.UiState = GameplayPhase.Title;
                break;
            case MenuCommand.Quit:
                if (_session.UiState == GameplayPhase.Title) Exit();
                break;
        }
    }

    private void ToggleCombatTrack()
    {
        var shouldPlayCombat = _session.UiState == GameplayPhase.Playing;
        if (shouldPlayCombat == _combatTrackOn) return;
        _combatTrackOn = shouldPlayCombat;
        if (shouldPlayCombat) _audio.PlayCombatTrack(); else _audio.PlayMenuTrack();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 12, 16));

        if (_session.UiState == GameplayPhase.Title)
        {
            _spriteBatch.Begin();
            _menuView.Draw(_spriteBatch, _font,
                _gfx.PreferredBackBufferWidth,
                _gfx.PreferredBackBufferHeight,
                gameTime.TotalGameTime.TotalSeconds);
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        if (_session.UiState == GameplayPhase.Settings)
        {
            _spriteBatch.Begin();
            _settingsView.Draw(_spriteBatch, _font,
                _gfx.PreferredBackBufferWidth,
                _gfx.PreferredBackBufferHeight,
                _settingsState, _audio);
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        // LinearClamp: wall/спрайты масштабируются без «зернистого» Point-шума на больших текстурах (512² и т.д.).
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp,
            transformMatrix: _session.Camera.WorldTransform);
        _worldScene.Draw(
            _spriteBatch,
            _session.PlayArea,
            ActiveFloorTexture,
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
            _session.PlayerProjectiles,
            _session.FloorSplatters,
            _session.WallSparkles,
            gameTime.TotalGameTime.TotalSeconds,
            _playerFrameDisp,
            _npcFrameDisp,
            _entitySprites);
        _spriteBatch.End();

        _spriteBatch.Begin();
        _hudView.Draw(
            _spriteBatch,
            _session.Player,
            _session.Npc,
            _font,
            _gfx.PreferredBackBufferWidth,
            _gfx.PreferredBackBufferHeight,
            _session.Waves.IsSecretMeatGrinder ? -1 : _session.Waves.CurrentArena + 1,
            gameTime.TotalGameTime.TotalSeconds,
            _session.ArenaTimer,
            GameplayOrchestrator.ArenaTimerMaxSeconds);

        switch (_session.UiState)
        {
            case GameplayPhase.Paused:
                _menuView.DrawPauseOverlay(_spriteBatch, _font,
                    _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight);
                break;
            case GameplayPhase.GameOver:
                _menuView.DrawGameOverOverlay(_spriteBatch, _font,
                    _gfx.PreferredBackBufferWidth, _gfx.PreferredBackBufferHeight,
                    playerDead: _session.Player.IsDead,
                    victory: !_session.Player.IsDead && !_session.Npc.IsDead &&
                             (_session.Waves.AllArenasCleared || _session.SecretMeatVictory));
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Texture2D? ActiveFloorTexture => _floorTexProcedural ?? _floorTexContent;

    /// <summary>Пересобирает пол при смене <see cref="GameplaySessionState.ArenaLayoutSeed"/> (новый уровень).</summary>
    private void RefreshProceduralFloorIfNeeded()
    {
        var cfg = _session.FloorTexture;
        var fp = cfg.ComputeFingerprint();
        if (!cfg.UseProceduralFloor)
        {
            if (_floorTexProcedural != null)
            {
                _floorTexProcedural.Dispose();
                _floorTexProcedural = null;
            }

            _floorBuiltForArenaSeed = _session.ArenaLayoutSeed;
            _floorSettingsFingerprint = fp;
            return;
        }

        if (_floorBuiltForArenaSeed == _session.ArenaLayoutSeed
            && _floorSettingsFingerprint == fp
            && _floorTexProcedural != null)
            return;

        _floorTexProcedural?.Dispose();
        _floorTexProcedural =
            FloorTextureGenerator.GenerateLevel(GraphicsDevice, cfg, _session.ArenaLayoutSeed);
        _floorBuiltForArenaSeed = _session.ArenaLayoutSeed;
        _floorSettingsFingerprint = fp;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _floorTexProcedural?.Dispose();
            _floorTexProcedural = null;
        }

        base.Dispose(disposing);
    }
}

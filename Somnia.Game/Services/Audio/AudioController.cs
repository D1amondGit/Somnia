using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Somnia.Game.Services.Audio;

/// <summary>
/// Обёртка над MonoGame Audio. Отсутствующие ассеты игнорируются.
/// Музыка: <c>menu_track</c>, <c>combat-track</c>. SFX: <c>hit_sfx</c>, <c>explode_sfx</c>,
/// <c>blood-boom_sfx</c>, <c>shoot_sfx</c>.
/// </summary>
public sealed class AudioController
{
    private Song? _menuSong;
    private Song? _combatSong;
    private SoundEffect? _hitSfx;
    private SoundEffect? _explodeSfx;
    private SoundEffect? _bloodBoomSfx;
    private SoundEffect? _shootSfx;
    private bool _loaded;

    private float _masterVolume = 0.8f;
    private float _musicVolume = 0.55f;
    private float _sfxVolume = 0.8f;

    public bool IsLoaded => _loaded;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = MathHelper.Clamp(value, 0f, 1f);
            ApplyMusicVolume();
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = MathHelper.Clamp(value, 0f, 1f);
            ApplyMusicVolume();
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = MathHelper.Clamp(value, 0f, 1f);
    }

    public void LoadContent(ContentManager content)
    {
        TryLoadSong(content, "menu_track", ref _menuSong);
        // Файл в Content: combat-track.mp3 → ассет "combat-track"
        TryLoadSong(content, "combat-track", ref _combatSong);
        TryLoadSfx(content, "hit_sfx", ref _hitSfx);
        TryLoadSfx(content, "explode_sfx", ref _explodeSfx);
        TryLoadSfx(content, "blood-boom_sfx", ref _bloodBoomSfx);
        TryLoadSfx(content, "shoot_sfx", ref _shootSfx);

        MediaPlayer.IsRepeating = true;
        ApplyMusicVolume();
        _loaded = true;
    }

    public void PlayMenuTrack() => SafePlay(_menuSong);

    public void PlayCombatTrack() => SafePlay(_combatSong);

    public void Stop()
    {
        try { MediaPlayer.Stop(); } catch { }
    }

    public void PlayHit() => SafePlayOnce(_hitSfx, volume: 0.65f);

    public void PlayExplosion() => SafePlayOnce(_explodeSfx, volume: 0.85f);

    public void PlayBloodBoom() => SafePlayOnce(_bloodBoomSfx, volume: 0.75f);

    public void PlayShoot() => SafePlayOnce(_shootSfx, volume: 0.55f);

    private void ApplyMusicVolume()
    {
        try { MediaPlayer.Volume = _masterVolume * _musicVolume; } catch { }
    }

    private static void SafePlay(Song? song)
    {
        if (song == null) return;
        try
        {
            if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != song)
                MediaPlayer.Play(song);
        }
        catch { }
    }

    private void SafePlayOnce(SoundEffect? sfx, float volume)
    {
        if (sfx == null) return;
        try { sfx.Play(MathHelper.Clamp(volume * _sfxVolume * _masterVolume, 0f, 1f), 0f, 0f); } catch { }
    }

    private static void TryLoadSong(ContentManager content, string asset, ref Song? slot)
    {
        try { slot = content.Load<Song>(asset); } catch { slot = null; }
    }

    private static void TryLoadSfx(ContentManager content, string asset, ref SoundEffect? slot)
    {
        try { slot = content.Load<SoundEffect>(asset); } catch { slot = null; }
    }
}

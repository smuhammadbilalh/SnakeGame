using Plugin.Maui.Audio;

namespace SnakeGame.Services;

public class AudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _backgroundMusicPlayer;
    private bool _soundEnabled = true;

    // Audio file paths
    private const string BONUS_FOOD_SOUND = "Bonus_Food_TimerInterval.wav";
    private const string GAME_OVER_SOUND = "Game_Over.wav";
    private const string HOME_SCREEN_MUSIC = "Home_Screen_playingcontinuously.wav";
    private const string NORMAL_FOOD_SOUND = "NormalFood_Eaten.wav";
    private const string COLLISION_SOUND = "SnakeCollideWithWallorSelf.wav";
    private const string GAME_START_SOUND = "Game_Start.wav";  // ⭐ NEW

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public void SetSoundEnabled(bool enabled)
    {
        _soundEnabled = enabled;
        if (!enabled)
        {
            StopBackgroundMusic();
        }
    }

    public async Task PlayBackgroundMusicAsync()
    {
        if (!_soundEnabled) return;

        try
        {
            StopBackgroundMusic();
            _backgroundMusicPlayer = _audioManager.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync(HOME_SCREEN_MUSIC));
            _backgroundMusicPlayer.Loop = true;
            _backgroundMusicPlayer.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error playing background music: {ex.Message}");
        }
    }

    public void StopBackgroundMusic()
    {
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.Stop();
            _backgroundMusicPlayer.Dispose();
            _backgroundMusicPlayer = null;
        }
    }

    // ⭐ NEW METHOD
    public async Task PlayGameStartAsync()
    {
        await PlaySoundEffectAsync(GAME_START_SOUND);
    }

    public async Task PlayNormalFoodEatenAsync()
    {
        await PlaySoundEffectAsync(NORMAL_FOOD_SOUND);
    }

    public async Task PlayBonusFoodSpawnAsync()
    {
        await PlaySoundEffectAsync(BONUS_FOOD_SOUND);
    }

    public async Task PlayCollisionAsync()
    {
        await PlaySoundEffectAsync(COLLISION_SOUND);
    }

    public async Task PlayGameOverAsync()
    {
        await PlaySoundEffectAsync(GAME_OVER_SOUND);
    }

    private async Task PlaySoundEffectAsync(string fileName)
    {
        if (!_soundEnabled) return;

        try
        {
            var player = _audioManager.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync(fileName));
            player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error playing sound {fileName}: {ex.Message}");
        }
    }
}

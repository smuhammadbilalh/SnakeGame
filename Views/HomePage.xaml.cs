using SnakeGame.Models;
using Mopups.Services;
using System.Collections.ObjectModel;

namespace SnakeGame.Views;

public partial class HomePage : ContentPage
{
    private ObservableCollection<GameMenuOption> _menuItems;
    private GameDifficulty _selectedDifficulty = GameDifficulty.Easy;
    private SnakeGameMode _selectedGameMode = SnakeGameMode.Classic;
    private bool _wallsEnabled = true;
    private int _speedLevel = 2;
    private bool _soundEnabled = true;
    private bool _gridEnabled = true;

    public HomePage()
    {
        InitializeComponent();
        InitializeMenuItems();
        MenuCollection.ItemsSource = _menuItems;
        UpdateSettingsDisplay();
    }

    private void InitializeMenuItems()
    {
        _menuItems = new ObservableCollection<GameMenuOption>
        {
            new GameMenuOption
            {
                IconGlyph = "\ue037", // play_arrow
                Title = "Start Game",
                Description = "Begin your snake adventure",
                CurrentValue = "▶",
                Type = MenuType.StartGame
            },
            new GameMenuOption
            {
                IconGlyph = "\ue9e4", // speed
                Title = "Level",
                Description = "Select difficulty",
                CurrentValue = "Easy",
                Type = MenuType.Level
            },
            new GameMenuOption
            {
                IconGlyph = "\ue30e", // gamepad
                Title = "Game Mode",
                Description = "Classic, Survival, Stages",
                CurrentValue = "Classic",
                Type = MenuType.GameMode
            },
            new GameMenuOption
            {
                IconGlyph = "\ue8b8", // settings
                Title = "Settings",
                Description = "Sound, Speed, Grid",
                CurrentValue = "⚙",
                Type = MenuType.Settings
            },
            new GameMenuOption
            {
                IconGlyph = "\ue24e", // emoji_events
                Title = "High Scores",
                Description = "View your best scores",
                CurrentValue = "🏆",
                Type = MenuType.HighScores
            }
        };
    }

    private async void OnMenuSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is GameMenuOption selected)
        {
            MenuCollection.SelectedItem = null;
            switch (selected.Type)
            {
                case MenuType.StartGame:
                    await Navigation.PushAsync(new GamePage(
                        _selectedDifficulty,
                        _selectedGameMode,
                        _wallsEnabled,
                        _speedLevel,
                        _gridEnabled));
                    break;
                case MenuType.Level:
                    await ShowLevelDialog();
                    break;
                case MenuType.GameMode:
                    await ShowGameModeDialog();
                    break;
                case MenuType.Settings:
                    await ShowSettingsDialog();
                    break;
                case MenuType.HighScores:
                    await ShowHighScoresDialog();
                    break;
            }
        }
    }

    private async Task ShowLevelDialog()
    {
        var options = new List<SelectionPopup.OptionItem>
        {
            new() { IconGlyph = "\ue838", Text = "Easy", Value = GameDifficulty.Easy },
            new() { IconGlyph = "\ue839", Text = "Medium", Value = GameDifficulty.Medium },
            new() { IconGlyph = "\ue83a", Text = "Hard", Value = GameDifficulty.Hard }
        };

        var popup = new SelectionPopup("Select Difficulty Level", options);
        await MopupService.Instance.PushAsync(popup);
        await popup.WaitForPopupToCloseAsync();

        if (popup.SelectedValue is GameDifficulty difficulty)
        {
            _selectedDifficulty = difficulty;
            _menuItems[1].CurrentValue = difficulty.ToString();
            UpdateSettingsDisplay();
        }
    }

    private async Task ShowGameModeDialog()
    {
        var options = new List<SelectionPopup.OptionItem>
        {
            new() { IconGlyph = "\ue338", Text = "Classic", Value = SnakeGameMode.Classic },
            new() { IconGlyph = "\uf197", Text = "Survival", Value = SnakeGameMode.Survival },
            new() { IconGlyph = "\uf8e6", Text = "Stages (Campaign)", Value = SnakeGameMode.Stages },
            new() { IconGlyph = "\ue028", Text = "Without Walls", Value = SnakeGameMode.NoWalls },
            new() { IconGlyph = "\ue3be", Text = "With Obstacles", Value = SnakeGameMode.Obstacles }
        };

        var popup = new SelectionPopup("Select Game Mode", options);
        await MopupService.Instance.PushAsync(popup);
        await popup.WaitForPopupToCloseAsync();

        if (popup.SelectedValue is SnakeGameMode mode)
        {
            _selectedGameMode = mode;
            _wallsEnabled = mode != SnakeGameMode.NoWalls;
            _menuItems[2].CurrentValue = mode.ToString();
            UpdateSettingsDisplay();
        }
    }

    private async Task ShowSettingsDialog()
    {
        var options = new List<SelectionPopup.OptionItem>
        {
            new() { IconGlyph = _soundEnabled ? "\ue050" : "\ue04f",
                   Text = $"Sound: {(_soundEnabled ? "ON" : "OFF")}",
                   Value = "sound" },
            new() { IconGlyph = _gridEnabled ? "\ue3ec" : "\ue3eb",
                   Text = $"Grid: {(_gridEnabled ? "ON" : "OFF")}",
                   Value = "grid" },
            new() { IconGlyph = "\ue9e4", Text = "Speed Settings", Value = "speed" },
            new() { IconGlyph = "\ue40a", Text = "Theme", Value = "theme" }
        };

        var popup = new SelectionPopup("Game Settings", options);
        await MopupService.Instance.PushAsync(popup);
        await popup.WaitForPopupToCloseAsync();

        if (popup.SelectedValue is string setting)
        {
            switch (setting)
            {
                case "sound":
                    _soundEnabled = !_soundEnabled;
                    break;
                case "grid":
                    _gridEnabled = !_gridEnabled;
                    break;
                case "speed":
                    await ShowSpeedDialog();
                    return;
            }
            UpdateSettingsDisplay();
        }
    }

    private async Task ShowSpeedDialog()
    {
        var options = new List<SelectionPopup.OptionItem>
        {
            new() { IconGlyph = "\ue536", Text = "Slow (Speed 1)", Value = 1 },
            new() { IconGlyph = "\ue566", Text = "Normal (Speed 2)", Value = 2 },
            new() { IconGlyph = "\ue52f", Text = "Fast (Speed 3)", Value = 3 },
            new() { IconGlyph = "\ue3e7", Text = "Very Fast (Speed 4)", Value = 4 }
        };

        var popup = new SelectionPopup("Select Speed", options);
        await MopupService.Instance.PushAsync(popup);
        await popup.WaitForPopupToCloseAsync();

        if (popup.SelectedValue is int speed)
        {
            _speedLevel = speed;
            UpdateSettingsDisplay();
        }
    }

    private async Task ShowHighScoresDialog()
    {
        var options = new List<SelectionPopup.OptionItem>
        {
            new() { IconGlyph = "\ue838", Text = "Hard Mode: 450 pts", Value = null },
            new() { IconGlyph = "\ue839", Text = "Medium Mode: 380 pts", Value = null },
            new() { IconGlyph = "\ue83a", Text = "Easy Mode: 320 pts", Value = null }
        };

        var popup = new SelectionPopup("🏆 High Scores", options);
        await MopupService.Instance.PushAsync(popup);
    }

    private void UpdateSettingsDisplay()
    {
       SettingsFooter.Text = $"Level: {_selectedDifficulty} | Mode: {_selectedGameMode} | Walls: {(_wallsEnabled ? "On" : "Off")} | Speed: {_speedLevel}";
    }
}

public static class MopupExtensions
{
    public static Task WaitForPopupToCloseAsync(this Mopups.Pages.PopupPage page)
    {
        var tcs = new TaskCompletionSource<bool>();
        page.Disappearing += (s, e) => tcs.TrySetResult(true);
        return tcs.Task;
    }
}

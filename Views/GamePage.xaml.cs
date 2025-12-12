using Microsoft.Maui.Graphics;
using SnakeGame.Drawables;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.Views;

public partial class GamePage : ContentPage
{
    private GameEngine _gameEngine;
    private IDispatcherTimer _gameTimer;
    private GameDrawable _gameDrawable;
    private bool _isInitialized = false;
    private readonly HighScoreManager _highScoreManager;

    public GameDifficulty Difficulty { get; }
    public SnakeGameMode GameMode { get; }
    public bool WallsEnabled { get; }
    public int SpeedLevel { get; }
    public bool GridEnabled { get; }

    public GamePage(GameDifficulty difficulty, SnakeGameMode gameMode = SnakeGameMode.Classic,
                    bool wallsEnabled = true, int speedLevel = 2, bool gridEnabled = true)
    {
        InitializeComponent();
        Difficulty = difficulty;
        GameMode = gameMode;
        WallsEnabled = wallsEnabled;
        SpeedLevel = speedLevel;
        GridEnabled = gridEnabled;
        _highScoreManager = new HighScoreManager();

        GameCanvas.SizeChanged += OnCanvasSizeChanged;

        // Add keyboard support for Windows desktop
        SetupKeyboardHandling();
    }

    private void SetupKeyboardHandling()
    {
        // Only setup keyboard for Windows desktop platform
#if WINDOWS
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            Microsoft.Maui.Handlers.PageHandler.Mapper.AppendToMapping("KeyboardHandler", (handler, view) =>
            {
                if (view is GamePage)
                {
                    handler.PlatformView.PreviewKeyDown += (s, e) =>
                    {
                        var key = e.Key;
                        Direction? newDirection = key switch
                        {
                            Windows.System.VirtualKey.Left or Windows.System.VirtualKey.A => Direction.Left,
                            Windows.System.VirtualKey.Right or Windows.System.VirtualKey.D => Direction.Right,
                            Windows.System.VirtualKey.Up or Windows.System.VirtualKey.W => Direction.Up,
                            Windows.System.VirtualKey.Down or Windows.System.VirtualKey.S => Direction.Down,
                            _ => null
                        };

                        if (newDirection.HasValue && _gameEngine != null && 
                            !_gameEngine.GameState.IsGameOver && !_gameEngine.GameState.IsPaused)
                        {
                            if (!IsOppositeDirection(newDirection.Value, _gameEngine.GameState.Snake.CurrentDirection))
                            {
                                _gameEngine.GameState.Snake.NextDirection = newDirection.Value;
                                e.Handled = true;
                            }
                        }
                        
                        // Pause/Unpause with Space or Escape
                        if (key == Windows.System.VirtualKey.Space || key == Windows.System.VirtualKey.Escape)
                        {
                            if (_gameEngine != null && !_gameEngine.GameState.IsGameOver)
                            {
                                _gameEngine.TogglePause();
                                UpdateUI();
                                GameCanvas.Invalidate();
                                e.Handled = true;
                            }
                        }
                    };
                }
            });
        }
#endif
    }

    private void OnCanvasSizeChanged(object sender, EventArgs e)
    {
        if (_isInitialized || GameCanvas.Width <= 0 || GameCanvas.Height <= 0)
            return;

        _isInitialized = true;
        InitializeGame();
    }

    private void InitializeGame()
    {
        const int CELL_SIZE = 20;
        int gridWidth = (int)(GameCanvas.Width / CELL_SIZE);
        int gridHeight = (int)(GameCanvas.Height / CELL_SIZE);

        gridWidth = Math.Max(15, gridWidth);
        gridHeight = Math.Max(15, gridHeight);

        // Use level dimensions for Stages mode
        if (GameMode == SnakeGameMode.Stages)
        {
            var levels = Level.GetNokiaLevels();
            if (levels.Count > 0)
            {
                gridWidth = levels[0].GridWidth;
                gridHeight = levels[0].GridHeight;
            }
        }

        _gameEngine = new GameEngine(gridWidth, gridHeight, Difficulty, GameMode, WallsEnabled);
        _gameEngine.LevelCompleted += OnLevelCompleted;
        _gameEngine.GameOverEvent += OnGameOver;

        _gameDrawable = new GameDrawable(_gameEngine, CELL_SIZE, GridEnabled);
        GameCanvas.Drawable = _gameDrawable;

        var interval = CalculateGameSpeed();

        _gameTimer = Dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(interval);
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();

        AddSwipeGestures();
        AddTapGesture();
        UpdateUI();
        GameCanvas.Invalidate();
    }

    private int CalculateGameSpeed()
    {
        var baseSpeed = Difficulty switch
        {
            GameDifficulty.Easy => 300,
            GameDifficulty.Medium => 200,
            GameDifficulty.Hard => 150,
            _ => 200
        };

        // Adjust by speed level
        var adjustment = (SpeedLevel - 2) * 50;
        return Math.Max(80, baseSpeed - adjustment);
    }

    private void AddTapGesture()
    {
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (_gameEngine.GameState.IsGameOver)
            {
                ShowGameOverOptions();
            }
            else
            {
                _gameEngine.TogglePause();
                UpdateUI();
                GameCanvas.Invalidate();
            }
        };
        GameCanvas.GestureRecognizers.Add(tapGesture);
    }

    private void AddSwipeGestures()
    {
        var leftSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        leftSwipe.Swiped += OnSwiped;
        var rightSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        rightSwipe.Swiped += OnSwiped;
        var upSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
        upSwipe.Swiped += OnSwiped;
        var downSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
        downSwipe.Swiped += OnSwiped;

        GameCanvas.GestureRecognizers.Add(leftSwipe);
        GameCanvas.GestureRecognizers.Add(rightSwipe);
        GameCanvas.GestureRecognizers.Add(upSwipe);
        GameCanvas.GestureRecognizers.Add(downSwipe);
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (_gameEngine.GameState.IsGameOver || _gameEngine.GameState.IsPaused)
            return;

        var newDirection = e.Direction switch
        {
            SwipeDirection.Left => Direction.Left,
            SwipeDirection.Right => Direction.Right,
            SwipeDirection.Up => Direction.Up,
            SwipeDirection.Down => Direction.Down,
            _ => _gameEngine.GameState.Snake.CurrentDirection
        };

        if (!IsOppositeDirection(newDirection, _gameEngine.GameState.Snake.CurrentDirection))
        {
            _gameEngine.GameState.Snake.NextDirection = newDirection;
        }
    }

    private bool IsOppositeDirection(Direction direction1, Direction direction2)
    {
        return (direction1 == Direction.Left && direction2 == Direction.Right) ||
               (direction1 == Direction.Right && direction2 == Direction.Left) ||
               (direction1 == Direction.Up && direction2 == Direction.Down) ||
               (direction1 == Direction.Down && direction2 == Direction.Up);
    }

    private void OnGameTick(object sender, EventArgs e)
    {
        _gameEngine.Update();
        UpdateUI();
        GameCanvas.Invalidate();
    }

    private async void OnLevelCompleted(object sender, EventArgs e)
    {
        _gameTimer.Stop();

        var result = await DisplayAlert("Level Complete!",
            $"Congratulations! You completed Level {_gameEngine.GameState.CurrentLevelNumber}\nScore: {_gameEngine.GameState.Score}",
            "Next Level", "Quit");

        if (result)
        {
            _gameEngine.AdvanceToNextLevel();
            _gameTimer.Start();
        }
        else
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnGameOver(object sender, EventArgs e)
    {
        _gameTimer.Stop();

        if (_highScoreManager.IsHighScore(_gameEngine.GameState.Score, GameMode))
        {
            var initials = await DisplayPromptAsync("New High Score!",
                $"Score: {_gameEngine.GameState.Score}\nEnter your initials:",
                maxLength: 3, keyboard: Keyboard.Text);

            _highScoreManager.AddHighScore(new HighScore
            {
                Score = _gameEngine.GameState.Score,
                Difficulty = Difficulty,
                GameMode = GameMode,
                Level = _gameEngine.GameState.CurrentLevelNumber,
                Date = DateTime.Now,
                PlayerInitials = initials?.ToUpper() ?? "AAA"
            });
        }

        ShowGameOverOptions();
    }

    private async void ShowGameOverOptions()
    {
        var action = await DisplayActionSheet("Game Over", "Main Menu", null, "Play Again", "View Scores");

        switch (action)
        {
            case "Play Again":
                _isInitialized = false;
                InitializeGame();
                break;
            case "View Scores":
                var topScores = _highScoreManager.GetTopScoresByMode(GameMode, 5);
                var scoreText = string.Join("\n", topScores.Select((s, i) => $"{i + 1}. {s.PlayerInitials} - {s.Score}"));
                await DisplayAlert("Top Scores", scoreText, "OK");
                await Navigation.PopAsync();
                break;
            default:
                await Navigation.PopAsync();
                break;
        }
    }

    private void UpdateUI()
    {
        var state = _gameEngine.GameState;
        var scoreText = $"Score: {state.Score}";

        if (GameMode == SnakeGameMode.Stages && state.CurrentLevel != null)
        {
            scoreText += $" | Level {state.CurrentLevelNumber}: {state.CurrentLevel.Name}";
            scoreText += $" | Target: {state.CurrentLevel.TargetScore}";
        }

        if (state.IsPaused)
        {
            scoreText += " | PAUSED";
        }

        ScoreLabel.Text = scoreText;
    }

    protected override void OnDisappearing()
    {
        _gameTimer?.Stop();
        base.OnDisappearing();
    }
}

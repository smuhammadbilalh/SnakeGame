namespace SnakeGame.Models;

public class GameState
{
    public Snake Snake { get; set; }
    public Food? CurrentFood { get; set; }
    public int Score { get; set; }
    public int RegularDotsEaten { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public SnakeGameMode GameMode { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsPaused { get; set; }
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public Level? CurrentLevel { get; set; }
    public int CurrentLevelNumber { get; set; } = 1;
    public List<Position> Obstacles { get; set; } = new();
    public bool WallsEnabled { get; set; } = true;
    public bool GridEnabled { get; set; } = true;

    public GameState(int gridWidth, int gridHeight, GameDifficulty difficulty, SnakeGameMode gameMode, bool wallsEnabled = true)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        Difficulty = difficulty;
        GameMode = gameMode;
        WallsEnabled = wallsEnabled || gameMode != SnakeGameMode.Classic;

        var startPos = new Position(gridWidth / 2, gridHeight / 2);
        Snake = new Snake(startPos);
        Score = 0;
        RegularDotsEaten = 0;
        IsGameOver = false;
        IsPaused = false;
        CurrentFood = null;

        // Generate walls for Complex mode
        if (gameMode == SnakeGameMode.Complex)
        {
            GenerateComplexWalls();
        }
    }

    public void LoadLevel(int levelNumber)
    {
        var levels = Level.GetNokiaLevels();
        if (levelNumber <= levels.Count)
        {
            CurrentLevel = levels[levelNumber - 1];
            CurrentLevelNumber = levelNumber;
            Obstacles = new List<Position>(CurrentLevel.Obstacles);
        }
    }

    public bool CheckLevelCompletion()
    {
        return false;
    }


    private void GenerateComplexWalls()
    {
        Obstacles = new List<Position>();

        // Better proportions for all screen sizes
        int margin = Math.Max(15, GridWidth / 10);  // Larger margin from edges
        int wallLength = Math.Min(GridWidth, GridHeight) / 2;  // Longer walls
        int gapSize = Math.Max(25, Math.Min(GridWidth, GridHeight) / 8);  // Larger gaps (proportional to screen)

        // Calculate wall positions with proper margins
        int topWallY = margin;
        int bottomWallY = GridHeight - margin;
        int leftWallX = margin;
        int rightWallX = GridWidth - margin;

        // Top wall (left side + right side with gap in middle)
        int topWallStart = margin + 5;
        int topWallEnd = GridWidth - margin - 5;
        int topGapStart = (topWallStart + topWallEnd) / 2 - gapSize / 2;
        int topGapEnd = topGapStart + gapSize;

        for (int x = topWallStart; x < topGapStart; x++)
        {
            Obstacles.Add(new Position(x, topWallY));
        }
        for (int x = topGapEnd; x < topWallEnd; x++)
        {
            Obstacles.Add(new Position(x, topWallY));
        }

        // Bottom wall (left side + right side with gap in middle)
        for (int x = topWallStart; x < topGapStart; x++)
        {
            Obstacles.Add(new Position(x, bottomWallY));
        }
        for (int x = topGapEnd; x < topWallEnd; x++)
        {
            Obstacles.Add(new Position(x, bottomWallY));
        }

        // Left wall (top side + bottom side with gap in middle)
        int leftWallStart = margin + 5;
        int leftWallEnd = GridHeight - margin - 5;
        int leftGapStart = (leftWallStart + leftWallEnd) / 2 - gapSize / 2;
        int leftGapEnd = leftGapStart + gapSize;

        for (int y = leftWallStart; y < leftGapStart; y++)
        {
            Obstacles.Add(new Position(leftWallX, y));
        }
        for (int y = leftGapEnd; y < leftWallEnd; y++)
        {
            Obstacles.Add(new Position(leftWallX, y));
        }

        // Right wall (top side + bottom side with gap in middle)
        for (int y = leftWallStart; y < leftGapStart; y++)
        {
            Obstacles.Add(new Position(rightWallX, y));
        }
        for (int y = leftGapEnd; y < leftWallEnd; y++)
        {
            Obstacles.Add(new Position(rightWallX, y));
        }
    }

    

    private bool IsNearSnakeStart(Position pos)
    {
        int centerX = GridWidth / 2;
        int centerY = GridHeight / 2;
        return Math.Abs(pos.X - centerX) < 5 && Math.Abs(pos.Y - centerY) < 5;
    }
}

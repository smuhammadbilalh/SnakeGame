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
        WallsEnabled = wallsEnabled || gameMode != SnakeGameMode.NoWalls;

        var startPos = new Position(gridWidth / 2, gridHeight / 2);
        Snake = new Snake(startPos);
        Score = 0;
        RegularDotsEaten = 0;
        IsGameOver = false;
        IsPaused = false;
        CurrentFood = null;

        if (gameMode == SnakeGameMode.Stages)
        {
            LoadLevel(1);
        }
        else if (gameMode == SnakeGameMode.Obstacles)
        {
            GenerateRandomObstacles();
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
        if (GameMode == SnakeGameMode.Stages && CurrentLevel != null)
        {
            return Score >= CurrentLevel.TargetScore;
        }
        return false;
    }

    private void GenerateRandomObstacles()
    {
        var random = new Random();
        int obstacleCount = (GridWidth * GridHeight) / 20;

        for (int i = 0; i < obstacleCount; i++)
        {
            Position pos;
            do
            {
                pos = new Position(random.Next(2, GridWidth - 2), random.Next(2, GridHeight - 2));
            } while (Obstacles.Contains(pos) || IsNearSnakeStart(pos));

            Obstacles.Add(pos);
        }
    }

    private bool IsNearSnakeStart(Position pos)
    {
        int centerX = GridWidth / 2;
        int centerY = GridHeight / 2;
        return Math.Abs(pos.X - centerX) < 5 && Math.Abs(pos.Y - centerY) < 5;
    }
}

namespace SnakeGame.Services;

using SnakeGame.Models;

public class GameEngine
{
    public GameState GameState { get; private set; }
    private readonly Random _random = new();

    public event EventHandler? LevelCompleted;
    public event EventHandler? GameOverEvent;

    public GameEngine(int gridWidth, int gridHeight, GameDifficulty difficulty, SnakeGameMode gameMode, bool wallsEnabled = true)
    {
        GameState = new GameState(gridWidth, gridHeight, difficulty, gameMode, wallsEnabled);
        SpawnFood();
    }

    public void Update()
    {
        if (GameState.IsGameOver || GameState.IsPaused)
            return;

        GameState.Snake.Move();

        // Check collisions based on game mode
        if (GameState.WallsEnabled && HasCollisionWithWalls())
        {
            EndGame();
            return;
        }

        // Wrap around if walls disabled (NoWalls mode)
        if (!GameState.WallsEnabled)
        {
            WrapSnakePosition();
        }

        // Check obstacle collision
        if (HasCollisionWithObstacles())
        {
            EndGame();
            return;
        }

        // Check self collision
        if (GameState.Snake.CollidesWithSelf())
        {
            EndGame();
            return;
        }

        HandleFoodCollision();
        HandleBonusExpiration();

        // Check level completion
        if (GameState.CheckLevelCompletion())
        {
            LevelCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AdvanceToNextLevel()
    {
        var levels = Level.GetNokiaLevels();
        if (GameState.CurrentLevelNumber < levels.Count)
        {
            GameState.LoadLevel(GameState.CurrentLevelNumber + 1);

            // Reset snake to center
            var startPos = new Position(GameState.GridWidth / 2, GameState.GridHeight / 2);
            GameState.Snake = new Snake(startPos);

            SpawnFood();
        }
        else
        {
            // Game completed all levels
            EndGame();
        }
    }

    public void TogglePause()
    {
        GameState.IsPaused = !GameState.IsPaused;
    }

    private void EndGame()
    {
        GameState.IsGameOver = true;
        GameOverEvent?.Invoke(this, EventArgs.Empty);
    }

    private void WrapSnakePosition()
    {
        var head = GameState.Snake.Head;
        var newHead = new Position(
            (head.X + GameState.GridWidth) % GameState.GridWidth,
            (head.Y + GameState.GridHeight) % GameState.GridHeight
        );

        if (newHead != head)
        {
            GameState.Snake.Body[0] = newHead;
        }
    }

    private bool HasCollisionWithWalls()
    {
        var head = GameState.Snake.Head;
        return head.X < 0 || head.Y < 0 ||
               head.X >= GameState.GridWidth ||
               head.Y >= GameState.GridHeight;
    }

    private bool HasCollisionWithObstacles()
    {
        return GameState.Obstacles.Contains(GameState.Snake.Head);
    }

    private void HandleFoodCollision()
    {
        if (GameState.CurrentFood == null)
            return;

        if (GameState.Snake.Head == GameState.CurrentFood.Position)
        {
            GameState.Snake.Grow();

            if (GameState.CurrentFood.Type == FoodType.Regular)
            {
                GameState.Score += 10;
                GameState.RegularDotsEaten += 1;
            }
            else
            {
                GameState.Score += 50;
            }

            SpawnFood();
        }
    }

    private void HandleBonusExpiration()
    {
        if (GameState.CurrentFood == null)
            return;

        if (GameState.CurrentFood.Type == FoodType.Bonus &&
            GameState.CurrentFood.IsExpired())
        {
            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        var type = FoodType.Regular;

        // Every 5 regular foods, spawn a bonus
        if (GameState.RegularDotsEaten > 0 && GameState.RegularDotsEaten % 5 == 0)
        {
            type = FoodType.Bonus;
            GameState.RegularDotsEaten = 0;
        }

        Position position;
        int attempts = 0;
        do
        {
            position = new Position(
                _random.Next(0, GameState.GridWidth),
                _random.Next(0, GameState.GridHeight));
            attempts++;
        } while ((GameState.Snake.Body.Contains(position) ||
                  GameState.Obstacles.Contains(position)) &&
                 attempts < 100);

        GameState.CurrentFood = new Food(position, type);
    }
}

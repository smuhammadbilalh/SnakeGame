namespace SnakeGame.Services;

using SnakeGame.Models;

public class GameEngine
{
    public GameState GameState { get; private set; }
    private readonly Random _random = new();

    private const int SNAKE_RADIUS = 3;
    private const int FOOD_RADIUS = 3;

    public event EventHandler<FoodType>? FoodEaten;
    public event EventHandler? CollisionDetected;
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

        // Handle based on game mode
        if (GameState.GameMode == SnakeGameMode.Walls)
        {
            // Die if hit walls
            if (HasCollisionWithWalls())
            {
                EndGame();
                return;
            }
        }
        else if (GameState.GameMode == SnakeGameMode.Classic)
        {
            // Wrap around
            WrapSnakePosition();
        }
        else if (GameState.GameMode == SnakeGameMode.Complex)
        {
            // Allow wrap around but check obstacle collision
            WrapSnakePosition();
        }

        // Check obstacle collision (for Complex mode)
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
            var startPos = new Position(GameState.GridWidth / 2, GameState.GridHeight / 2);
            GameState.Snake = new Snake(startPos);
            SpawnFood();
        }
        else
        {
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
        CollisionDetected?.Invoke(this, EventArgs.Empty);
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
        return head.X - SNAKE_RADIUS < 0 ||
               head.Y - SNAKE_RADIUS < 0 ||
               head.X + SNAKE_RADIUS >= GameState.GridWidth ||
               head.Y + SNAKE_RADIUS >= GameState.GridHeight;
    }

    private bool HasCollisionWithObstacles()
    {
        var head = GameState.Snake.Head;
        foreach (var obstacle in GameState.Obstacles)
        {
            var dx = Math.Abs(head.X - obstacle.X);
            var dy = Math.Abs(head.Y - obstacle.Y);

            if (dx <= SNAKE_RADIUS && dy <= SNAKE_RADIUS)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleFoodCollision()
    {
        if (GameState.CurrentFood == null)
            return;

        var head = GameState.Snake.Head;
        var food = GameState.CurrentFood.Position;

        var dx = head.X - food.X;
        var dy = head.Y - food.Y;
        var distanceSquared = dx * dx + dy * dy;

        var collisionRadius = SNAKE_RADIUS + FOOD_RADIUS;
        var collisionRadiusSquared = collisionRadius * collisionRadius;

        if (distanceSquared <= collisionRadiusSquared)
        {
            GameState.Snake.Grow();

            FoodEaten?.Invoke(this, GameState.CurrentFood.Type);

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
                _random.Next(FOOD_RADIUS + 1, GameState.GridWidth - FOOD_RADIUS - 1),
                _random.Next(FOOD_RADIUS + 1, GameState.GridHeight - FOOD_RADIUS - 1));
            attempts++;
        } while ((IsPositionOccupiedBySnake(position) ||
                  IsPositionOccupiedByObstacle(position) ||
                  IsTooCloseToSnakeHead(position)) &&
                 attempts < 100);

        GameState.CurrentFood = new Food(position, type);
    }

    private bool IsPositionOccupiedBySnake(Position position)
    {
        foreach (var segment in GameState.Snake.Body)
        {
            var dx = Math.Abs(segment.X - position.X);
            var dy = Math.Abs(segment.Y - position.Y);

            if (dx < SNAKE_RADIUS + FOOD_RADIUS && dy < SNAKE_RADIUS + FOOD_RADIUS)
                return true;
        }
        return false;
    }

    private bool IsPositionOccupiedByObstacle(Position position)
    {
        foreach (var obstacle in GameState.Obstacles)
        {
            var dx = Math.Abs(obstacle.X - position.X);
            var dy = Math.Abs(obstacle.Y - position.Y);
            if (dx < FOOD_RADIUS * 2 && dy < FOOD_RADIUS * 2)
                return true;
        }
        return false;
    }

    private bool IsTooCloseToSnakeHead(Position position)
    {
        var head = GameState.Snake.Head;
        var dx = Math.Abs(head.X - position.X);
        var dy = Math.Abs(head.Y - position.Y);

        return dx < 10 || dy < 10;
    }
}
namespace SnakeGame.Services;

using SnakeGame.Models;

public class GameEngine
{
    public GameState GameState { get; private set; }
    private readonly Random _random = new();

    // Visual sizes for accurate collision detection
    private const int SNAKE_RADIUS = 3; // Snake spans 6 cells (3 radius)
    private const int FOOD_RADIUS = 3;  // Food spans 6 cells (3 radius)

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

        // Check with radius offset for thick snake
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
            // Check if snake head overlaps with obstacle (within radius)
            var dx = Math.Abs(head.X - obstacle.X);
            var dy = Math.Abs(head.Y - obstacle.Y);

            // If within radius, collision detected
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

        // Circle-based collision: check if snake head overlaps with food
        var head = GameState.Snake.Head;
        var food = GameState.CurrentFood.Position;

        // Calculate distance between centers
        var dx = head.X - food.X;
        var dy = head.Y - food.Y;
        var distanceSquared = dx * dx + dy * dy;

        // Combined radius for collision (if any pixel touches, eat the food)
        var collisionRadius = SNAKE_RADIUS + FOOD_RADIUS;
        var collisionRadiusSquared = collisionRadius * collisionRadius;

        // If circles overlap, eat the food
        if (distanceSquared <= collisionRadiusSquared)
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

            // Check if food would overlap with snake body
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
        // Make sure food doesn't spawn too close to snake head
        var head = GameState.Snake.Head;
        var dx = Math.Abs(head.X - position.X);
        var dy = Math.Abs(head.Y - position.Y);

        // Food should be at least 10 cells away from snake head for fair gameplay
        return dx < 10 || dy < 10;
    }
}

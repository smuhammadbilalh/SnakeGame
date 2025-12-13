namespace SnakeGame.Models;

public enum SnakeGameMode
{
    Classic,    // No walls - wrap around
    Walls,      // Boundary walls - die if hit
    Complex     // 4 walls with gaps
}

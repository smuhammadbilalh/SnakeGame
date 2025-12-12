using Microsoft.Maui.Graphics;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.Drawables;

public class GameDrawable : IDrawable
{
    private readonly GameEngine _gameEngine;
    private readonly float _cellSize;
    private readonly bool _gridEnabled;

    public GameDrawable(GameEngine gameEngine, float cellSize, bool gridEnabled = true)
    {
        _gameEngine = gameEngine;
        _cellSize = cellSize;
        _gridEnabled = gridEnabled;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromRgb(15, 23, 42); // Dark background
        canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

        var state = _gameEngine.GameState;

        // Draw grid
        if (_gridEnabled)
        {
            DrawGrid(canvas, state.GridWidth, state.GridHeight);
        }

        // Draw obstacles
        DrawObstacles(canvas, state.Obstacles);

        // Draw Food
        if (state.CurrentFood != null)
        {
            DrawFood(canvas, state.CurrentFood);
        }

        // Draw Snake
        DrawSnake(canvas, state.Snake);

        // Draw pause overlay
        if (state.IsPaused)
        {
            DrawPauseOverlay(canvas, dirtyRect);
        }

        // Draw Game Over overlay
        if (state.IsGameOver)
        {
            DrawGameOverOverlay(canvas, dirtyRect, state.Score);
        }
    }

    private void DrawGrid(ICanvas canvas, int gridWidth, int gridHeight)
    {
        canvas.StrokeColor = Color.FromRgba(255, 255, 255, 20);
        canvas.StrokeSize = 1;

        for (int x = 0; x <= gridWidth; x++)
        {
            canvas.DrawLine(x * _cellSize, 0, x * _cellSize, gridHeight * _cellSize);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            canvas.DrawLine(0, y * _cellSize, gridWidth * _cellSize, y * _cellSize);
        }
    }

    private void DrawObstacles(ICanvas canvas, List<Position> obstacles)
    {
        canvas.FillColor = Color.FromRgb(71, 85, 105); // Gray

        foreach (var obstacle in obstacles)
        {
            var x = obstacle.X * _cellSize;
            var y = obstacle.Y * _cellSize;
            canvas.FillRoundedRectangle(x + 1, y + 1, _cellSize - 2, _cellSize - 2, 3);
        }
    }

    private void DrawSnake(ICanvas canvas, Snake snake)
    {
        for (int i = 0; i < snake.Body.Count; i++)
        {
            var segment = snake.Body[i];
            var x = segment.X * _cellSize;
            var y = segment.Y * _cellSize;

            if (i == 0) // Head
            {
                canvas.FillColor = Color.FromRgb(52, 211, 153); // Bright green
                canvas.FillRoundedRectangle(x + 1, y + 1, _cellSize - 2, _cellSize - 2, 6);

                // Draw eyes
                canvas.FillColor = Colors.White;
                var eyeSize = _cellSize / 6;
                var eyeOffset = _cellSize / 3;

                if (snake.CurrentDirection == Direction.Right || snake.CurrentDirection == Direction.Left)
                {
                    canvas.FillCircle(x + eyeOffset, y + eyeOffset, eyeSize);
                    canvas.FillCircle(x + eyeOffset, y + _cellSize - eyeOffset, eyeSize);
                }
                else
                {
                    canvas.FillCircle(x + eyeOffset, y + eyeOffset, eyeSize);
                    canvas.FillCircle(x + _cellSize - eyeOffset, y + eyeOffset, eyeSize);
                }
            }
            else // Body
            {
                var greenIntensity = 220 - (i * 5);
                canvas.FillColor = Color.FromRgb(16, Math.Max(100, greenIntensity), 129);
                canvas.FillRoundedRectangle(x + 2, y + 2, _cellSize - 4, _cellSize - 4, 4);
            }
        }
    }

    private void DrawFood(ICanvas canvas, Food food)
    {
        var foodX = food.Position.X * _cellSize + _cellSize / 2;
        var foodY = food.Position.Y * _cellSize + _cellSize / 2;
        var radius = _cellSize / 2 - 2;

        if (food.Type == FoodType.Bonus)
        {
            // Bonus food - pulsing gold star
            canvas.FillColor = Color.FromRgb(251, 191, 36); // Gold
            DrawStar(canvas, foodX, foodY, radius, radius * 0.5f);

            // Draw timer arc
            var remaining = food.GetRemainingSeconds();
            if (remaining > 0)
            {
                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 2;
                var angle = (remaining / 5f) * 360;
                canvas.DrawArc(food.Position.X * _cellSize, food.Position.Y * _cellSize,
                              _cellSize, _cellSize, 0, angle, false, false);
            }
        }
        else
        {
            // Regular food - red apple
            canvas.FillColor = Color.FromRgb(239, 68, 68); // Red
            canvas.FillCircle(foodX, foodY, radius);

            // Add shine
            canvas.FillColor = Color.FromRgba(255, 255, 255, 128);
            canvas.FillCircle(foodX - radius / 3, foodY - radius / 3, radius / 3);
        }
    }

    private void DrawStar(ICanvas canvas, float cx, float cy, float outerRadius, float innerRadius)
    {
        var points = new List<PointF>();
        for (int i = 0; i < 10; i++)
        {
            var radius = i % 2 == 0 ? outerRadius : innerRadius;
            var angle = (i * 36 - 90) * Math.PI / 180;
            points.Add(new PointF(
                cx + (float)(radius * Math.Cos(angle)),
                cy + (float)(radius * Math.Sin(angle))
            ));
        }

        var path = new PathF();
        path.MoveTo(points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            path.LineTo(points[i]);
        }
        path.Close();

        canvas.FillPath(path);
    }

    private void DrawPauseOverlay(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromRgba(0, 0, 0, 180);
        canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 48;
        canvas.DrawString("PAUSED", dirtyRect.Width / 2, dirtyRect.Height / 2 - 30,
                         HorizontalAlignment.Center);

        canvas.FontSize = 20;
        canvas.DrawString("Tap to Resume", dirtyRect.Width / 2, dirtyRect.Height / 2 + 30,
                         HorizontalAlignment.Center);
    }

    private void DrawGameOverOverlay(ICanvas canvas, RectF dirtyRect, int score)
    {
        canvas.FillColor = Color.FromRgba(0, 0, 0, 200);
        canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

        canvas.FontColor = Color.FromRgb(239, 68, 68); // Red
        canvas.FontSize = 56;
        canvas.DrawString("GAME OVER", dirtyRect.Width / 2, dirtyRect.Height / 2 - 60,
                         HorizontalAlignment.Center);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 32;
        canvas.DrawString($"Final Score: {score}", dirtyRect.Width / 2, dirtyRect.Height / 2,
                         HorizontalAlignment.Center);

        canvas.FontSize = 18;
        canvas.DrawString("Tap for Options", dirtyRect.Width / 2, dirtyRect.Height / 2 + 50,
                         HorizontalAlignment.Center);
    }
}
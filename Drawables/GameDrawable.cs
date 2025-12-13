using Microsoft.Maui.Graphics;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.Drawables;

public class GameDrawable : IDrawable
{
    private readonly GameEngine _gameEngine;
    private readonly float _cellSize;
    private readonly bool _gridEnabled;
    private const int SNAKE_THICKNESS = 6; // Snake = 6 cells to match food diameter
    private const int FOOD_RADIUS = 3; // Food radius = 3 cells (6 diameter)

    public GameDrawable(GameEngine gameEngine, float cellSize, bool gridEnabled = true)
    {
        _gameEngine = gameEngine;
        _cellSize = cellSize;
        _gridEnabled = gridEnabled;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromRgb(15, 23, 42);
        canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

        var state = _gameEngine.GameState;

        // Skip grid for ultra-fine cells
        if (_gridEnabled && _cellSize >= 8)
        {
            DrawGrid(canvas, state.GridWidth, state.GridHeight);
        }

        DrawObstacles(canvas, state.Obstacles);

        if (state.CurrentFood != null)
        {
            DrawFood(canvas, state.CurrentFood);
        }

        DrawSnake(canvas, state.Snake);

        if (state.IsPaused)
        {
            DrawPauseOverlay(canvas, dirtyRect);
        }

        if (state.IsGameOver)
        {
            DrawGameOverOverlay(canvas, dirtyRect, state.Score);
        }
    }

    private void DrawGrid(ICanvas canvas, int gridWidth, int gridHeight)
    {
        canvas.StrokeColor = Color.FromRgba(255, 255, 255, 15);
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
        canvas.FillColor = Color.FromRgb(71, 85, 105);

        foreach (var obstacle in obstacles)
        {
            var x = obstacle.X * _cellSize;
            var y = obstacle.Y * _cellSize;
            canvas.FillRectangle(x, y, _cellSize, _cellSize);
        }
    }

    private void DrawSnake(ICanvas canvas, Snake snake)
    {
        // Snake thickness matches food diameter for perfect eating
        var halfThickness = SNAKE_THICKNESS / 2.0f;

        for (int i = 0; i < snake.Body.Count; i++)
        {
            var segment = snake.Body[i];

            // Center the snake segment
            var centerX = (segment.X + 0.5f) * _cellSize;
            var centerY = (segment.Y + 0.5f) * _cellSize;

            var segmentSize = SNAKE_THICKNESS * _cellSize;
            var segmentX = centerX - (halfThickness * _cellSize);
            var segmentY = centerY - (halfThickness * _cellSize);

            if (i == 0) // Head - same size as body for smooth eating
            {
                // Main head color - bright green
                canvas.FillColor = Color.FromRgb(52, 211, 153);
                canvas.FillRoundedRectangle(segmentX, segmentY, segmentSize, segmentSize, _cellSize * 1.5f);

                // Draw eyes based on direction
                canvas.FillColor = Colors.White;
                var eyeSize = _cellSize * 1.2f;
                var eyeOffset = _cellSize * 1.5f;

                if (snake.CurrentDirection == Direction.Right)
                {
                    canvas.FillCircle(centerX + eyeOffset, centerY - eyeOffset * 0.5f, eyeSize);
                    canvas.FillCircle(centerX + eyeOffset, centerY + eyeOffset * 0.5f, eyeSize);

                    canvas.FillColor = Colors.Black;
                    canvas.FillCircle(centerX + eyeOffset + _cellSize * 0.3f, centerY - eyeOffset * 0.5f, eyeSize * 0.5f);
                    canvas.FillCircle(centerX + eyeOffset + _cellSize * 0.3f, centerY + eyeOffset * 0.5f, eyeSize * 0.5f);
                }
                else if (snake.CurrentDirection == Direction.Left)
                {
                    canvas.FillCircle(centerX - eyeOffset, centerY - eyeOffset * 0.5f, eyeSize);
                    canvas.FillCircle(centerX - eyeOffset, centerY + eyeOffset * 0.5f, eyeSize);

                    canvas.FillColor = Colors.Black;
                    canvas.FillCircle(centerX - eyeOffset - _cellSize * 0.3f, centerY - eyeOffset * 0.5f, eyeSize * 0.5f);
                    canvas.FillCircle(centerX - eyeOffset - _cellSize * 0.3f, centerY + eyeOffset * 0.5f, eyeSize * 0.5f);
                }
                else if (snake.CurrentDirection == Direction.Up)
                {
                    canvas.FillCircle(centerX - eyeOffset * 0.5f, centerY - eyeOffset, eyeSize);
                    canvas.FillCircle(centerX + eyeOffset * 0.5f, centerY - eyeOffset, eyeSize);

                    canvas.FillColor = Colors.Black;
                    canvas.FillCircle(centerX - eyeOffset * 0.5f, centerY - eyeOffset - _cellSize * 0.3f, eyeSize * 0.5f);
                    canvas.FillCircle(centerX + eyeOffset * 0.5f, centerY - eyeOffset - _cellSize * 0.3f, eyeSize * 0.5f);
                }
                else if (snake.CurrentDirection == Direction.Down)
                {
                    canvas.FillCircle(centerX - eyeOffset * 0.5f, centerY + eyeOffset, eyeSize);
                    canvas.FillCircle(centerX + eyeOffset * 0.5f, centerY + eyeOffset, eyeSize);

                    canvas.FillColor = Colors.Black;
                    canvas.FillCircle(centerX - eyeOffset * 0.5f, centerY + eyeOffset + _cellSize * 0.3f, eyeSize * 0.5f);
                    canvas.FillCircle(centerX + eyeOffset * 0.5f, centerY + eyeOffset + _cellSize * 0.3f, eyeSize * 0.5f);
                }
            }
            else // Body segments
            {
                // Gradient color - darker towards tail
                var greenIntensity = 220 - (i * 2);
                canvas.FillColor = Color.FromRgb(16, Math.Max(100, greenIntensity), 129);

                // Rounded body segments
                canvas.FillRoundedRectangle(segmentX, segmentY, segmentSize, segmentSize, _cellSize);

                // Add highlight for 3D effect
                if (i < snake.Body.Count / 2)
                {
                    canvas.FillColor = Color.FromRgba(255, 255, 255, 30);
                    canvas.FillRoundedRectangle(segmentX + _cellSize, segmentY + _cellSize,
                                               segmentSize * 0.3f, segmentSize * 0.3f, _cellSize * 0.5f);
                }
            }
        }
    }

    private void DrawFood(ICanvas canvas, Food food)
    {
        // Food size matches snake thickness for smooth eating
        var foodRadius = FOOD_RADIUS * _cellSize;
        var foodX = (food.Position.X + 0.5f) * _cellSize; // Center on cell
        var foodY = (food.Position.Y + 0.5f) * _cellSize;

        if (food.Type == FoodType.Bonus)
        {
            // Bonus food - large gold star
            canvas.FillColor = Color.FromRgb(251, 191, 36);
            DrawStar(canvas, foodX, foodY, foodRadius, foodRadius * 0.6f);

            // Shine effect
            canvas.FillColor = Color.FromRgba(255, 255, 255, 150);
            DrawStar(canvas, foodX - _cellSize, foodY - _cellSize, foodRadius * 0.4f, foodRadius * 0.25f);

            // Timer arc
            var remaining = food.GetRemainingSeconds();
            if (remaining > 0)
            {
                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 3;
                var angle = (remaining / 5f) * 360;
                canvas.DrawArc(foodX - foodRadius - 3, foodY - foodRadius - 3,
                              (foodRadius + 3) * 2, (foodRadius + 3) * 2, 0, angle, false, false);
            }
        }
        else
        {
            // Regular food - red circle (same diameter as snake thickness)
            canvas.FillColor = Color.FromRgb(239, 68, 68);
            canvas.FillCircle(foodX, foodY, foodRadius);

            // Shine effect - top-left
            canvas.FillColor = Color.FromRgba(255, 255, 255, 180);
            canvas.FillCircle(foodX - foodRadius * 0.3f, foodY - foodRadius * 0.3f, foodRadius * 0.4f);

            // Secondary shine
            canvas.FillColor = Color.FromRgba(255, 255, 255, 100);
            canvas.FillCircle(foodX + foodRadius * 0.2f, foodY + foodRadius * 0.2f, foodRadius * 0.2f);
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

        canvas.FontColor = Color.FromRgb(239, 68, 68);
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

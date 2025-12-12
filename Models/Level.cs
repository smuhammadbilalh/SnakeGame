namespace SnakeGame.Models;

public class Level
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TargetScore { get; set; }
    public List<Position> Obstacles { get; set; } = new();
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public int Speed { get; set; }
    public Color WallColor { get; set; } = Colors.Gray;

    public static List<Level> GetNokiaLevels()
    {
        return new List<Level>
        {
            new Level { Number = 1, Name = "Beginner", TargetScore = 50, GridWidth = 15, GridHeight = 15, Speed = 300, Obstacles = new() },
            new Level { Number = 2, Name = "Easy Street", TargetScore = 100, GridWidth = 18, GridHeight = 18, Speed = 250, Obstacles = CreateCenterBox(18, 18) },
            new Level { Number = 3, Name = "Cross Road", TargetScore = 150, GridWidth = 20, GridHeight = 20, Speed = 220, Obstacles = CreateCrossPattern(20, 20) },
            new Level { Number = 4, Name = "The Maze", TargetScore = 200, GridWidth = 22, GridHeight = 22, Speed = 200, Obstacles = CreateMazePattern(22, 22) },
            new Level { Number = 5, Name = "Corners", TargetScore = 250, GridWidth = 22, GridHeight = 22, Speed = 180, Obstacles = CreateCornerBoxes(22, 22) },
            new Level { Number = 6, Name = "Tunnel", TargetScore = 300, GridWidth = 25, GridHeight = 15, Speed = 170, Obstacles = CreateTunnelPattern(25, 15) },
            new Level { Number = 7, Name = "Spiral", TargetScore = 350, GridWidth = 24, GridHeight = 24, Speed = 150, Obstacles = CreateSpiralPattern(24, 24) },
            new Level { Number = 8, Name = "Columns", TargetScore = 400, GridWidth = 25, GridHeight = 20, Speed = 140, Obstacles = CreateColumnPattern(25, 20) },
            new Level { Number = 9, Name = "Champion", TargetScore = 500, GridWidth = 28, GridHeight = 28, Speed = 120, Obstacles = CreateChampionPattern(28, 28) },
        };
    }

    private static List<Position> CreateCenterBox(int width, int height)
    {
        var obstacles = new List<Position>();
        int centerX = width / 2;
        int centerY = height / 2;

        for (int i = -3; i <= 3; i++)
        {
            obstacles.Add(new Position(centerX + i, centerY - 3));
            obstacles.Add(new Position(centerX + i, centerY + 3));
            obstacles.Add(new Position(centerX - 3, centerY + i));
            obstacles.Add(new Position(centerX + 3, centerY + i));
        }
        return obstacles;
    }

    private static List<Position> CreateCrossPattern(int width, int height)
    {
        var obstacles = new List<Position>();
        int centerX = width / 2;
        int centerY = height / 2;

        for (int i = -5; i <= 5; i++)
        {
            obstacles.Add(new Position(centerX + i, centerY));
            obstacles.Add(new Position(centerX, centerY + i));
        }
        return obstacles;
    }

    private static List<Position> CreateMazePattern(int width, int height)
    {
        var obstacles = new List<Position>();

        for (int y = 5; y < height - 5; y += 3)
        {
            for (int x = 4; x < width - 4; x += 6)
            {
                obstacles.Add(new Position(x, y));
                obstacles.Add(new Position(x + 1, y));
            }
        }
        return obstacles;
    }

    private static List<Position> CreateCornerBoxes(int width, int height)
    {
        var obstacles = new List<Position>();
        int size = 4;

        // Top-left
        for (int y = 2; y < 2 + size; y++)
            for (int x = 2; x < 2 + size; x++)
                obstacles.Add(new Position(x, y));

        // Top-right
        for (int y = 2; y < 2 + size; y++)
            for (int x = width - 2 - size; x < width - 2; x++)
                obstacles.Add(new Position(x, y));

        // Bottom-left
        for (int y = height - 2 - size; y < height - 2; y++)
            for (int x = 2; x < 2 + size; x++)
                obstacles.Add(new Position(x, y));

        // Bottom-right
        for (int y = height - 2 - size; y < height - 2; y++)
            for (int x = width - 2 - size; x < width - 2; x++)
                obstacles.Add(new Position(x, y));

        return obstacles;
    }

    private static List<Position> CreateTunnelPattern(int width, int height)
    {
        var obstacles = new List<Position>();
        int tunnelY1 = height / 3;
        int tunnelY2 = 2 * height / 3;

        for (int x = 5; x < width - 5; x++)
        {
            if (x % 8 < 5)
            {
                obstacles.Add(new Position(x, tunnelY1));
                obstacles.Add(new Position(x, tunnelY2));
            }
        }
        return obstacles;
    }

    private static List<Position> CreateSpiralPattern(int width, int height)
    {
        var obstacles = new List<Position>();
        int cx = width / 2;
        int cy = height / 2;

        for (int r = 3; r < Math.Min(width, height) / 2 - 2; r += 3)
        {
            for (int angle = 0; angle < 270; angle += 15)
            {
                int x = cx + (int)(r * Math.Cos(angle * Math.PI / 180));
                int y = cy + (int)(r * Math.Sin(angle * Math.PI / 180));
                if (x >= 0 && x < width && y >= 0 && y < height)
                    obstacles.Add(new Position(x, y));
            }
        }
        return obstacles;
    }

    private static List<Position> CreateColumnPattern(int width, int height)
    {
        var obstacles = new List<Position>();

        for (int x = 5; x < width - 5; x += 5)
        {
            for (int y = 3; y < height - 3; y++)
            {
                if (y < height / 3 || y > 2 * height / 3)
                    obstacles.Add(new Position(x, y));
            }
        }
        return obstacles;
    }

    private static List<Position> CreateChampionPattern(int width, int height)
    {
        var obstacles = new List<Position>();

        // Combine multiple patterns
        obstacles.AddRange(CreateCrossPattern(width, height));
        obstacles.AddRange(CreateCornerBoxes(width, height));

        return obstacles.Distinct().ToList();
    }
}
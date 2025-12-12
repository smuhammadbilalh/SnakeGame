namespace SnakeGame.Models;

public class HighScore
{
    public int Score { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public SnakeGameMode GameMode { get; set; }
    public int Level { get; set; }
    public DateTime Date { get; set; }
    public string PlayerInitials { get; set; } = "AAA";
}

public class HighScoreManager
{
    private const string HighScoreKey = "snake_highscores";
    private List<HighScore> _highScores = new();

    public HighScoreManager()
    {
        LoadHighScores();
    }

    public List<HighScore> GetTopScores(int count = 10)
    {
        return _highScores
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToList();
    }

    public List<HighScore> GetTopScoresByMode(SnakeGameMode mode, int count = 5)
    {
        return _highScores
            .Where(s => s.GameMode == mode)
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToList();
    }

    public bool IsHighScore(int score, SnakeGameMode mode)
    {
        var topScores = GetTopScoresByMode(mode, 10);
        return topScores.Count < 10 || score > topScores.Last().Score;
    }

    public void AddHighScore(HighScore highScore)
    {
        _highScores.Add(highScore);
        SaveHighScores();
    }

    private void LoadHighScores()
    {
        var json = Preferences.Get(HighScoreKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                _highScores = System.Text.Json.JsonSerializer.Deserialize<List<HighScore>>(json) ?? new();
            }
            catch
            {
                _highScores = new List<HighScore>();
            }
        }
    }

    private void SaveHighScores()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_highScores);
        Preferences.Set(HighScoreKey, json);
    }
}
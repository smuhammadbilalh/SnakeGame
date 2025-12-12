namespace SnakeGame.Models;

public class GameMenuOption
{
    public string IconGlyph { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CurrentValue { get; set; }
    public MenuType Type { get; set; }
}

public enum MenuType
{
    StartGame,
    Level,
    GameMode,
    Settings,
    HighScores
}
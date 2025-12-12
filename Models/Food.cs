namespace SnakeGame.Models;

public class Food
{
    public Position Position { get; set; }
    public FoodType Type { get; set; }
    public DateTime? SpawnTime { get; set; }
    public int BonusTimeSeconds { get; set; } = 5;

    public Food(Position position, FoodType type)
    {
        Position = position;
        Type = type;
        if (type == FoodType.Bonus)
        {
            SpawnTime = DateTime.Now;
        }
    }

    public bool IsExpired()
    {
        if (Type == FoodType.Regular || SpawnTime == null)
            return false;

        return (DateTime.Now - SpawnTime.Value).TotalSeconds >= BonusTimeSeconds;
    }

    public int GetRemainingSeconds()
    {
        if (Type == FoodType.Regular || SpawnTime == null)
            return 0;

        var elapsed = (DateTime.Now - SpawnTime.Value).TotalSeconds;
        return Math.Max(0, BonusTimeSeconds - (int)elapsed);
    }
}

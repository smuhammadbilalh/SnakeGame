namespace SnakeGame.Models;

public class Snake
{
    public List<Position> Body { get; set; } = new();
    public Direction CurrentDirection { get; set; }
    public Direction NextDirection { get; set; }

    public Snake(Position startPosition)
    {
        Body.Add(startPosition);
        Body.Add(new Position(startPosition.X - 1, startPosition.Y));
        Body.Add(new Position(startPosition.X - 2, startPosition.Y));
        CurrentDirection = Direction.Right;
        NextDirection = Direction.Right;
    }

    public Position Head => Body[0];

    public void Move()
    {
        CurrentDirection = NextDirection;

        var newHead = CurrentDirection switch
        {
            Direction.Up => new Position(Head.X, Head.Y - 1),
            Direction.Down => new Position(Head.X, Head.Y + 1),
            Direction.Left => new Position(Head.X - 1, Head.Y),
            Direction.Right => new Position(Head.X + 1, Head.Y),
            _ => Head
        };

        Body.Insert(0, newHead);
        Body.RemoveAt(Body.Count - 1);
    }

    public void Grow()
    {
        // Grow by 3 segments instead of 1 for more visible growth
        var tail = Body[^1];
        Body.Add(tail);
        Body.Add(tail);
        Body.Add(tail);
    }

    public bool CollidesWithSelf()
    {
        return Body.Skip(1).Any(segment => segment == Head);
    }
}
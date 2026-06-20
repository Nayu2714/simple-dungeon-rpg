public class Player
{
    public int Y { get; private set; }
    public int X { get; private set; }
    
    public Player(int y, int x)
    {
        Y = y;
        X = x;
    }
    
    public void MoveTo(int y, int x)
    {
        Y = y;
        X = x;
    }
}
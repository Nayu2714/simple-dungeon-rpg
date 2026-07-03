namespace simple_dungeon_rpg.World;

public class Room
{
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    
    /*
     *       .
     *  #####|#####
     *  #.........#
     *  #.....@...|.
     * .-..E......#
     *  #.........#
     *  ####-######
     *      .
     */

    public Room(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
    
    public (int y, int x) Center => ((Top + Bottom) / 2, (Left + Right) / 2);

    public (int y, int x) RandomPoint(Random rng)
    {
        return (rng.Next(Top+1, Bottom), rng.Next(Left+1, Right));
    }
}
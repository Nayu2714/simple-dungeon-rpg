public class Map
{
    private readonly Tile[,] tiles;
    
    public int Height { get; }
    public int Width { get; }
    
    public List<Entity> Entities { get; }

    public Map(string[] source)
    {
        Height = source.Length;
        Width = source.Max(str => str.Length);
        
        tiles = new Tile[Height, Width];
        
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                tiles[y, x] = source[y][x] switch
                {
                    '.' => Tile.Floor,
                    '#' => Tile.Wall,
                    _   => Tile.Empty
                };
            }
    }

    private Map(Tile[,] tiles)
    {
        this.tiles = tiles;
        Height = tiles.GetLength(0);
        Width = tiles.GetLength(1);
    }

    public static Map Generate(int height, int width)
    {
        height = Math.Max(3, height);
        width = Math.Max(3, width);
        
        var tiles = new Tile[height,width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isEdge = y == 0 || y == height - 1 || x == 0 || x == width - 1;
                tiles[y, x] = isEdge ? Tile.Wall : Tile.Floor;
            }
        }
        
        return new Map(tiles);
    }
    
    public char GetTile(int y, int x)
    {
        return tiles[y, x] switch
        {
            Tile.Floor => '.',
            Tile.Wall => '#',
            _ => ' '
        };
    }

    public bool CanMoveTo(int y, int x)
    {
        return x >= 0 && y >= 0 &&
               x < Width && y < Height &&
               tiles[y, x] == Tile.Floor;
    }
}
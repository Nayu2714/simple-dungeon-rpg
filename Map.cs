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

    /*
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
    */

    public static Map Generate(int height, int width)
    {
        var tiles = new Tile[height, width];
        for(int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tiles[y, x] = Tile.Wall;
        var rootDivision = new MapDivision();
        rootDivision.Set(0,0,width,height);
        
        rootDivision.Split(3);
        
        var divisions = new List<MapDivision>();
        rootDivision.CollectDivisions(divisions);

        foreach (var div in divisions)
        {
            for (int y = div.Top + 1; y < div.Bottom - 1; y++)
            {
                for (int x = div.Left + 1; x < div.Right - 1; x++)
                {
                    tiles[y, x] = Tile.Floor;
                }
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

    public class MapDivision
    {
        private static readonly Random random = new Random();
        
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }

        public void Set(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Height
        {
            get => Bottom - Top;
        }
        
        public int Width
        {
            get => Right - Left;
        }
        
        public MapDivision? ChildA { get; private set; }
        public MapDivision? ChildB { get; private set; }

        public void Split(int count)
        {
            if (count <= 0) return;
            if (Height <= 8 || Width <= 8) return;
            
            ChildA = new MapDivision();
            ChildB = new MapDivision();
            int mid;
            
            if (Width > Height) // 幅＞高さ → 縦に区画を割る
            {
                mid = Left + random.Next((int)Math.Round(Width * 0.4), (int)Math.Round(Width * 0.6) + 1);

                ChildA.Set(this.Left, this.Top, mid, this.Bottom);
                ChildB.Set(mid, this.Top, this.Right, this.Bottom);
            }
            else // 幅＜高さ → 横に区画を割る
            {
                mid = Top + random.Next((int)Math.Round(Height * 0.4), (int)Math.Round(Height * 0.6) + 1);

                ChildA.Set(this.Left, this.Top, this.Right, mid);
                ChildB.Set(this.Left, mid, this.Right, this.Bottom);
            }
            
            ChildA.Split(count - 1);
            ChildB.Split(count - 1);
        }

        public void CollectDivisions(List<MapDivision> divisions)
        {
            if (ChildA == null && ChildB == null)
            {
                divisions.Add(this);
                return;
            }
            ChildA?.CollectDivisions(divisions);
            ChildB?.CollectDivisions(divisions);
        }
    }
}
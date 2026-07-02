public class Map
{
    private readonly Tile[,] tiles;
    
    public int Height { get; }
    public int Width { get; }
    
    public List<Entity> Entities { get; }
    
    public IReadOnlyList<Room> Rooms { get; }

    public (int y, int x) PlayerStartPos { get; private set; }

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
                    '>' => Tile.DownStairs,
                    '<' => Tile.UpStairs,
                    _   => Tile.Empty
                };
            }
    }

    private Map(Tile[,] tiles, (int startY, int startX) playerStartPos, IReadOnlyList<Room> rooms)
    {
        this.tiles = tiles;
        Height = tiles.GetLength(0);
        Width = tiles.GetLength(1);
        PlayerStartPos = playerStartPos;
        Rooms = rooms;
    }

    public static Map Generate(int height, int width)
    {
        var tiles = new Tile[height, width];
        for(int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tiles[y, x] = Tile.Wall;
        
        var rootDiv = new MapDivision();
        rootDiv.Set(0,0,width,height);
        
        var divs = rootDiv.Divide(5);

        foreach (var div in divs)
        {
            div.MakeRoom();
            var room = div.Room;
            if (room == null) continue;
            
            for (int y = room.Top; y < room.Bottom; y++)
            {
                for (int x = room.Left; x < room.Right; x++)
                {
                    tiles[y, x] = Tile.Floor;
                }
            }
        }
        
        var corridors = new List<((int y, int x) a, (int y, int x) b)>();
        rootDiv.CollectRoomCenters(corridors);

        foreach (var (a, b) in corridors)
        {
            SetHCorridor(tiles, a.y, a.x, b.x);
            SetWCorridor(tiles, b.x, a.y, b.y);
        }
        
        var rooms = divs.Select(div => div.Room!).ToList();

        var playerStartPos = rooms[0].Center;
        
        return new Map(tiles, playerStartPos,rooms);
    }
    
    public char GetTile(int y, int x)
    {
        return tiles[y, x] switch
        {
            Tile.Floor => '.',
            Tile.Wall => '#',
            Tile.DownStairs => '>',
            Tile.UpStairs => '<',
            _ => ' '
        };
    }
    
    public bool CanMoveTo(int y, int x)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;

        return tiles[y, x] switch
        {
            Tile.Floor => true,
            Tile.UpStairs => true,
            Tile.DownStairs => true,
            _ => false
        };
    }

    public void SetDownStairs(int y, int x)
    {
        if(tiles[y, x] == Tile.Floor) tiles[y, x] = Tile.DownStairs;
    }

    private static void SetHCorridor(Tile[,] tiles, int y, int x1, int x2)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
        {
            tiles[y, x] = Tile.Floor;
        }
    }
    
    private static void SetWCorridor(Tile[,] tiles, int x, int y1, int y2)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
        {
            tiles[y, x] = Tile.Floor;
        }
    }

    public class MapDivision
    {
        private static readonly Random Rng = new Random();
        
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

        public Room? Room { get; private set; }

        public MapDivision? ChildA { get; private set; }
        public MapDivision? ChildB { get; private set; }

        public void MakeRoom()
        {
            int maxRoomWidth = Width - 2;
            int maxRoomHeight = Height - 2;
            int minRoomWidth = Math.Max(4, maxRoomWidth / 2);
            int minRoomHeight = Math.Max(4, maxRoomHeight / 2);
            if (maxRoomWidth < 4 || maxRoomHeight < 4)
            {
                Room = new Room(Left+1, Top+1, Right-1, Bottom-1);
                return;
            }
                        
            int roomWidth = Rng.Next(minRoomWidth, maxRoomWidth + 1);
            int roomHeight = Rng.Next(minRoomHeight, maxRoomHeight + 1);
            int roomLeft = Left + Rng.Next(1, Width - roomWidth);
            int roomTop = Top + Rng.Next(1, Height - roomHeight);

            Room = new Room(roomLeft, roomTop, roomLeft + roomWidth, roomTop + roomHeight);
        }

        public List<MapDivision> Divide(int count) // count = 欲しい区画の数
                {
                    List<MapDivision> divs = new List<MapDivision>() { this };
        
                    for (int i = 0; i < count-1; i++)
                    {
                        MapDivision? maxDiv = divs.MaxBy(d => d.Width * d.Height);
                        if (maxDiv == null || maxDiv.TrySplit() == false) break;
        
                        divs.Remove(maxDiv);
                        divs.Add(maxDiv.ChildA!);
                        divs.Add(maxDiv.ChildB!);
                    }
        
                    return divs;
                }

        /*
        public (int y, int x) RoomCenter()
        {
            return ( (Room!.Top + Room.Bottom) / 2, (Room.Left + Room.Right) / 2 );
        }
        */
        
        public bool TrySplit()
        {
            if (Height <= 8 || Width <= 8) return false;
            
            ChildA = new MapDivision();
            ChildB = new MapDivision();
            int mid;
            
            if (Width > Height) // 幅＞高さ → 縦に区画を割る
            {
                mid = Left + Rng.Next((int)Math.Round(Width * 0.4), (int)Math.Round(Width * 0.6) + 1);
                ChildA.Set(this.Left, this.Top, mid, this.Bottom);
                ChildB.Set(mid, this.Top, this.Right, this.Bottom);
            }
            else // 幅＜高さ → 横に区画を割る
            {
                mid = Top + Rng.Next((int)Math.Round(Height * 0.4), (int)Math.Round(Height * 0.6) + 1);

                ChildA.Set(this.Left, this.Top, this.Right, mid);
                ChildB.Set(this.Left, mid, this.Right, this.Bottom);
            }

            return true;
        }
        
        public (int y, int x) CollectRoomCenters(List<((int y, int x) a, (int y, int x) b)> centers)
        {
            if (ChildA == null && ChildB == null)
            {
                return Room!.Center;
            }
            
            var aCenter = ChildA!.CollectRoomCenters(centers);
            var bCenter = ChildB!.CollectRoomCenters(centers);
            centers.Add((aCenter, bCenter));

            return aCenter;
        }
    }
}
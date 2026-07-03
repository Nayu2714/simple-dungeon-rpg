using System.Text;
using simple_dungeon_rpg.Entities;
using simple_dungeon_rpg.Items;
using simple_dungeon_rpg.World;

namespace simple_dungeon_rpg;

class Program
{
    static readonly int NewLineLength = Environment.NewLine.Length;
    
    static void Main()
    {
        Console.Clear();
        Console.CursorVisible = false;

        var rng = new Random();
        
        /*
        string[] mapSource =
        {
            "......#..",
            ".###.....",
            ".#....#..",
            ".##.#....",
            ".#......."
        };
        */
        
        Map map = Map.Generate(20,40, rng);

        (int y, int x) downStairsPos;
        int attempts = 0;
        do
        {
            downStairsPos = map.Rooms[rng.Next(map.Rooms.Count)].RandomPoint(rng);
            attempts++;
        }
        while (downStairsPos == map.PlayerStartPos && attempts < 100);
        attempts = 0;
        
        (int y, int x) upStairsPos = map.PlayerStartPos;
        
        map.SetDownStairs(downStairsPos.y, downStairsPos.x);
        map.SetUpStairs(upStairsPos.y, upStairsPos.x);
        
        Player player = new Player(map.PlayerStartPos.y, map.PlayerStartPos.x);
        
        List<Enemy> enemies = new List<Enemy>();
        int enemyNums = 5;
        for (int i = 1; i <= enemyNums; i++)
        {
            (int y, int x) pos;
            do
            {
                pos = map.Rooms[rng.Next(0, map.Rooms.Count)].RandomPoint(rng);
                attempts++;
            }
            while((!map.CanMoveTo(pos.y, pos.x) || GetEnemyAt(enemies, pos.y, pos.x) != null || pos == map.PlayerStartPos) && attempts < 100);
            attempts = 0;
            enemies.Add(new Enemy("Enemy", pos.y, pos.x));
        }
        
        /*
        enemies.Add(new Enemy("Enemy", 0, 0));
        enemies.Add(new Enemy("Enemy", 1, 4));
        enemies.Add(new Enemy("Enemy", 4, 7));
        */
        
        List<(Item item, int y, int x)> floorItems = new List<(Item item, int y, int x)>();
        List<Item> initItems = new List<Item>();
        initItems.Add(new Potion("HealPotion", 20));
        initItems.Add(new Potion("HealPotion", 20));
        initItems.Add(new Weapon("IronSword", 5));
        foreach(var item in initItems)
        {
            (int y, int x) pos;
            do
            {
                pos = map.Rooms[rng.Next(0, map.Rooms.Count)].RandomPoint(rng);
                attempts++;
            } while ((!map.CanMoveTo(pos.y, pos.x) || GetItemAt(floorItems, pos.y, pos.x) != null ||
                      pos == map.PlayerStartPos) && attempts < 100);
            attempts = 0;
            floorItems.Add((item, pos.y, pos.x));
        }
        
        Console.WriteLine("【W/A/S/D】移動・攻撃 | 【Q】ゲーム終了");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine();
        
        var logs = new List<string>();

        // マップ描画用 描画開始位置
        (int row, int col) mapOriginCursor = (Console.CursorTop, Console.CursorLeft);

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        bool isRunning = true;
        while (isRunning)
        {
            Draw(map, player, enemies, floorItems, mapOriginCursor, logs);
            
            // 入力部
            ConsoleKeyInfo inputKey = Console.ReadKey(true);

            int dir = -1;
            switch (inputKey.Key)
            {
                case ConsoleKey.W: dir = 0; break;
                case ConsoleKey.S: dir = 1; break;
                case ConsoleKey.A: dir = 2; break;
                case ConsoleKey.D: dir = 3; break;
                case ConsoleKey.Q: isRunning = false; break;
            }

            if (dir != -1)
            {
                int nextY = player.Y + dy[dir];
                int nextX = player.X + dx[dir];
                
                Enemy? target = GetEnemyAt(enemies, nextY, nextX);

                if (target != null)
                {
                    int dmg = player.Atk;
                    target.TakeDamage(dmg);
                    logs.Add($"プレイヤーの攻撃！ {target.Name} に {dmg} ダメージ");
                    
                    if (target.IsDead)
                    {
                        enemies.Remove(target);
                        logs.Add($"{target.Name} を倒した！");
                    }
                }
                else if(map.CanMoveTo(nextY, nextX))
                {
                    player.MoveTo(nextY, nextX);
                }

                int[,] dist = BuildDistanceMap(map, player.Y, player.X);
                
                var orderedEnemies = enemies.OrderBy(e => dist[e.Y,e.X]).ToList();
                    
                foreach(Enemy enemy in orderedEnemies)
                {
                    if (IsAdjusent(player.Y, player.X, enemy.Y, enemy.X))
                    {
                        player.TakeDamage(enemy.Atk);
                        logs.Add($"{enemy.Name} の攻撃！ {enemy.Atk} ダメージ");
                        continue;
                    }
                    
                    int bestY = enemy.Y;
                    int bestX = enemy.X;
                    int bestDist = dist[bestY, bestX];

                    for (int d = 0; d < 4; d++)
                    {
                        int ny = enemy.Y + dy[d];
                        int nx = enemy.X + dx[d];
                        
                        if (ny < 0 || ny >= map.Height || nx < 0 || nx >= map.Width) continue;
                        if (dist[ny, nx] == -1) continue;
                        if (dist[ny, nx] >= bestDist) continue;
                        if (GetEnemyAt(enemies, ny, nx) != null) continue;
                        if (player.Y == ny && player.X == nx) continue;
                        
                        bestDist = dist[ny, nx];
                        bestY = ny;
                        bestX = nx;
                    }
                    enemy.MoveTo(bestY, bestX);
                }
            }
            if (player.IsDead) isRunning = false;
        }
        
        Console.CursorVisible = true;
        if(player.IsDead) logs.Add("ゲームオーバー！");
        logs.Add("ゲームを終了しました。");
        Draw(map, player, enemies, floorItems, mapOriginCursor, logs);
    }

    static void Draw(Map map, Player player, List<Enemy> enemies, List<(Item item, int y, int x)> floorItems, (int row, int col) origin, List<string> logs)
    {
        StringBuilder sb = new StringBuilder();
        
        // --- I. メイン画面（マップ） ---
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (player.Y == y && player.X == x && !player.IsDead)
                {
                    sb.Append(player.Symbol);
                }
                else
                {
                    Enemy? enemy = GetEnemyAt(enemies, y, x);
                    if (enemy != null)
                    {
                        sb.Append(enemy.Symbol);
                    }
                    else if (GetItemAt(floorItems, y, x) is Item item)
                    {
                        sb.Append(item.Symbol);
                    }
                    else
                    {
                        sb.Append(map.GetTile(y, x));
                    }
                }
            }
            sb.AppendLine();
        }
        sb.AppendLine();
        
        // --- II. ステータス ---
        string status = $"HP: {player.Hp} / {player.MaxHp}";
        sb.AppendLine(status.PadRight(27));
        sb.AppendLine();
        
        // --- III. ログ ---
        int logLines = 5;
        var recent = logs.Skip(Math.Max(0, logs.Count - logLines)).ToList();
        for (int i = 0; i < logLines; i++)
        {
            string line = i < recent.Count ? $"> {recent[i]}" : "";
            sb.AppendLine(line.PadRight(40));
        }
        
        Console.SetCursorPosition(origin.col, origin.row);
        Console.Write(sb.ToString());
    }

    static Enemy? GetEnemyAt(List<Enemy> enemies, int y, int x)
    {
        return enemies.FirstOrDefault(enemy => enemy.Y == y && enemy.X == x);
    }

    static Item? GetItemAt(List<(Item item, int y, int x)> floorItems, int y, int x)
    {
        return floorItems.FirstOrDefault(item => item.y == y && item.x == x).item;
    }

    static int[,] BuildDistanceMap(Map map, int startY, int startX)
    {
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };
        
        int[,] dist = new int[map.Height, map.Width];
        for (int i = 0; i < map.Height; i++)
            for (int j = 0; j < map.Width; j++)
                dist[i, j] = -1;
        
        var queue = new Queue<(int y, int x)>();
        dist[startY, startX] = 0;
        queue.Enqueue((startY, startX));

        while (queue.Count > 0)
        {
            var (y,x)  = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                int ny = y + dy[d];
                int nx = x+dx[d];
                if (ny >= 0 && ny < map.Height &&
                    nx >= 0 && nx < map.Width &&
                    map.GetTile(ny, nx) != '#' &&
                    dist[ny, nx] == -1)
                {
                    dist[ny, nx] = dist[y, x] + 1;
                    queue.Enqueue((ny, nx));
                }
            }
        }

        return dist;
    }

    static bool IsAdjusent(int ay, int ax, int by, int bx)
    {
        return Math.Abs(ax - bx) + Math.Abs(ay - by) == 1;
    }
}

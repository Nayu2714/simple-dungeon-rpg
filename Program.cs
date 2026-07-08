using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using simple_dungeon_rpg.Entities;
using simple_dungeon_rpg.Items;
using simple_dungeon_rpg.Items.Interfaces;
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
        int enemyNums = 0;
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

        // 描画用 描画開始位置
        (int row, int col) mapOriginCursor = (Console.CursorTop, Console.CursorLeft);
        (int row, int col) inventoryOriginCursor = (Console.CursorTop, Console.CursorLeft + 6);

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        map.ResetVisibility();
        FieldOfView.Compute(map, player.Y, player.X, player.VisionRadius);
        
        bool isRunning = true;
        while (isRunning)
        {
            Draw(map, player, enemies, floorItems, mapOriginCursor, logs);
            
            // 入力部
            ConsoleKeyInfo inputKey = Console.ReadKey(true);

            int dir = -1;
            bool get = false;
            bool inventory = false;
            
            switch (inputKey.Key)
            {
                case ConsoleKey.W: dir = 0; break;
                case ConsoleKey.S: dir = 1; break;
                case ConsoleKey.A: dir = 2; break;
                case ConsoleKey.D: dir = 3; break;
                case ConsoleKey.Spacebar: get = true; break;
                case ConsoleKey.Tab: inventory = true; break;
                
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
                
                ProcessEnemyTurn(map, player, enemies, logs);
                map.ResetVisibility();
                FieldOfView.Compute(map, player.Y, player.X, player.VisionRadius);
            }
            else if (get)
            {
                var hearItems = GetItemsAt(floorItems, player.Y, player.X);
                if (hearItems.Count > 0)
                {
                    foreach (var item in hearItems)
                    {
                        player.AddItem(item);
                    }
                    floorItems.RemoveAll(item => item.y == player.Y && item.x == player.X);
                }
            }

            int selectedIndex = 0;
            while (inventory)
            {
                DrawInventory(player, selectedIndex, inventoryOriginCursor);
                
                ConsoleKeyInfo inventoryInputKey = Console.ReadKey(true);

                int select = 0;
                bool enter = false;
                bool back = false;
                
                switch (inventoryInputKey.Key)
                {
                    case ConsoleKey.W: select = -1; break;
                    case ConsoleKey.S: select =  1; break;
                    case ConsoleKey.Enter or ConsoleKey.Spacebar: enter = true; break;
                    case ConsoleKey.Tab or ConsoleKey.Escape: back = true; break;
                    
                    case ConsoleKey.Q: isRunning = false; inventory = false; break;
                }

                if (select != 0)
                {
                    int equipCount = (player.Weapon != null ? 1 : 0);
                    selectedIndex = Math.Clamp(selectedIndex + select, 0, player.Inventory.Count + equipCount);
                }
                else if (enter)
                {
                    int equipNum = (player.Weapon != null ? 1 : 0);
                    if (selectedIndex == player.Inventory.Count + equipNum)
                    {
                        // (Close) が選択された時の処理
                        inventory = false;
                    }
                    else if (equipNum != 0 && selectedIndex == player.Inventory.Count)
                    {
                        // 装備行 が選択された時の処理
                        player.UnEquip();
                        inventory = false;
                        ProcessEnemyTurn(map, player, enemies, logs);
                        map.ResetVisibility();
                        FieldOfView.Compute(map, player.Y, player.X, player.VisionRadius);
                    }
                    else
                    {
                        // インベントリ内アイテム行 が選択されたときの処理
                        var item = player.Inventory[selectedIndex];
                        if (item is IUsable usable)
                        {
                            usable.Use(player);
                            if (usable.IsConsumable) player.RemoveItem(item);
                            ProcessEnemyTurn(map, player, enemies, logs);
                            map.ResetVisibility();
                            FieldOfView.Compute(map, player.Y, player.X, player.VisionRadius);
                        }
                        else if (item is IEquippable equippable)
                        {
                            player.Equip(equippable);
                            ProcessEnemyTurn(map, player, enemies, logs);
                            map.ResetVisibility();
                            FieldOfView.Compute(map, player.Y, player.X, player.VisionRadius);
                        }
                        // ---
                        inventory = false;
                    }
                }
                else if (back)
                {
                    inventory = false;
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
        // --- I. メイン画面（マップ） ---
        for (int y = 0; y < map.Height; y++)
        {
            Console.SetCursorPosition(origin.col, origin.row + y);
            
            for (int x = 0; x < map.Width; x++)
            {
                if (map.IsVisible(y, x) == false && map.IsExplored(y, x) == false)
                {
                    Console.Write(' ');
                }
                else if (map.IsVisible(y, x) == false && map.IsExplored(y, x) == true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(map.GetTile(y, x));
                }
                else if (map.IsVisible(y, x) == true)
                {
                    Console.ResetColor();
                    if (player.Y == y && player.X == x && !player.IsDead) 
                    {
                        Console.Write(player.Symbol);
                    }
                    else
                    {
                        Enemy? enemy = GetEnemyAt(enemies, y, x);
                        if (enemy != null)
                        {
                            Console.Write(enemy.Symbol);
                        }
                        else if (GetItemAt(floorItems, y, x) is Item item)
                        {
                            Console.Write(item.Symbol);
                        }
                        else
                        {
                            Console.Write(map.GetTile(y, x));
                        }
                    }
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.WriteLine();
        
        // --- II. ステータス・ログ ---
        StringBuilder sb = new StringBuilder();
        
        string status = $"HP: {player.Hp} / {player.MaxHp}";
        sb.AppendLine(status.PadRight(27));
        sb.AppendLine();
        
        int logLines = 5;
        var recent = logs.Skip(Math.Max(0, logs.Count - logLines)).ToList();
        for (int i = 0; i < logLines; i++)
        {
            string line = i < recent.Count ? $"> {recent[i]}" : "";
            sb.AppendLine(line.PadRight(40));
        }
        
        Console.SetCursorPosition(origin.col, origin.row + map.Height + 1);
        Console.Write(sb.ToString());
    }

    static void DrawInventory(Player player, int index, (int row, int col) origin)
    {
        IReadOnlyList<Item> inventory = player.Inventory;
        int equipNum = (player.Weapon != null ? 1 : 0);
        
        for (int i = 0; i < inventory.Count + 1 + equipNum; i++) // (close)行を追加するため、`inventory.Count + 1`にしている。
        {
            StringBuilder sb = new StringBuilder();
            
            if (i == index) sb.Append(">"); else sb.Append(" ");
            
            if (equipNum != 0 && i == inventory.Count)
            {
                sb.Append("E "+player.Weapon!.Name.PadRight(40));
            }
            else if (i == inventory.Count + equipNum)
            {
                sb.AppendLine("(close)".PadRight(40));
            }
            else
            {
                sb.AppendLine("  "+inventory[i].Name.PadRight(40));
            }
            
            Console.SetCursorPosition(origin.col, origin.row + i);
            Console.Write(sb.ToString());
        }
    }

    static void ProcessEnemyTurn(Map map, Player player, List<Enemy> enemies, List<string> logs)
    {
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

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
    
    static Enemy? GetEnemyAt(List<Enemy> enemies, int y, int x)
    {
        return enemies.FirstOrDefault(enemy => enemy.Y == y && enemy.X == x);
    }

    static Item? GetItemAt(List<(Item item, int y, int x)> floorItems, int y, int x)
    {
        return floorItems.FirstOrDefault(items => items.y == y && items.x == x).item;
    }
    
    static List<Item> GetItemsAt(List<(Item item, int y, int x)> floorItems, int y, int x)
    {
        return floorItems.Where(entry => entry.y == y && entry.x == x).Select(e => e.item).ToList();
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

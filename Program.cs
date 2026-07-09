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
        
        Floor currentFloor = Floor.Generate(rng, 1);
        
        Player player = new Player(currentFloor.Map.PlayerStartPos.y, currentFloor.Map.PlayerStartPos.x);
        
        Console.WriteLine("【W/A/S/D】移動・攻撃 | 【Q】ゲーム終了");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine();
        
        var logs = new List<string>();

        // 描画用 描画開始位置
        (int row, int col) mapOriginCursor = (Console.CursorTop, Console.CursorLeft);
        (int row, int col) inventoryOriginCursor = (Console.CursorTop, Console.CursorLeft + 6);

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        currentFloor.Map.ResetVisibility();
        FieldOfView.Compute(currentFloor.Map, player.Y, player.X, player.VisionRadius);
        
        bool isRunning = true;
        while (isRunning)
        {
            Draw(currentFloor, player, mapOriginCursor, logs);
            
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
                
                Enemy? target = currentFloor.GetEnemyAt(nextY, nextX);

                if (target != null)
                {
                    int dmg = player.Atk;
                    target.TakeDamage(dmg);
                    logs.Add($"プレイヤーの攻撃！ {target.Name} に {dmg} ダメージ");
                    
                    if (target.IsDead)
                    {
                        currentFloor.Enemies.Remove(target);
                        logs.Add($"{target.Name} を倒した！");
                    }
                }
                else if(currentFloor.Map.CanMoveTo(nextY, nextX))
                {
                    player.MoveTo(nextY, nextX);
                }
                
                ProcessEnemyTurn(currentFloor, player, logs);
                currentFloor.Map.ResetVisibility();
                FieldOfView.Compute(currentFloor.Map, player.Y, player.X, player.VisionRadius);
            }
            else if (get)
            {
                var hearItems = currentFloor.GetItemsAt(player.Y, player.X);
                if (hearItems.Count > 0)
                {
                    foreach (var item in hearItems)
                    {
                        player.AddItem(item);
                    }
                    currentFloor.FloorItems.RemoveAll(item => item.y == player.Y && item.x == player.X);
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
                        ProcessEnemyTurn(currentFloor, player, logs);
                        currentFloor.Map.ResetVisibility();
                        FieldOfView.Compute(currentFloor.Map, player.Y, player.X, player.VisionRadius);
                    }
                    else
                    {
                        // インベントリ内アイテム行 が選択されたときの処理
                        var item = player.Inventory[selectedIndex];
                        if (item is IUsable usable)
                        {
                            usable.Use(player);
                            if (usable.IsConsumable) player.RemoveItem(item);
                            ProcessEnemyTurn(currentFloor, player, logs);
                            currentFloor.Map.ResetVisibility();
                            FieldOfView.Compute(currentFloor.Map, player.Y, player.X, player.VisionRadius);
                        }
                        else if (item is IEquippable equippable)
                        {
                            player.Equip(equippable);
                            ProcessEnemyTurn(currentFloor, player, logs);
                            currentFloor.Map.ResetVisibility();
                            FieldOfView.Compute(currentFloor.Map, player.Y, player.X, player.VisionRadius);
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
        Draw(currentFloor, player, mapOriginCursor, logs);
    }

    static void Draw(Floor floor, Player player, (int row, int col) origin, List<string> logs)
    {
        // --- I. メイン画面（マップ） ---
        for (int y = 0; y < floor.Map.Height; y++)
        {
            Console.SetCursorPosition(origin.col, origin.row + y);
            
            for (int x = 0; x < floor.Map.Width; x++)
            {
                if (floor.Map.IsVisible(y, x) == false && floor.Map.IsExplored(y, x) == false)
                {
                    Console.Write(' ');
                }
                else if (floor.Map.IsVisible(y, x) == false && floor.Map.IsExplored(y, x) == true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(floor.Map.GetTile(y, x));
                }
                else if (floor.Map.IsVisible(y, x) == true)
                {
                    Console.ResetColor();
                    if (player.Y == y && player.X == x && !player.IsDead) 
                    {
                        Console.Write(player.Symbol);
                    }
                    else
                    {
                        Enemy? enemy = floor.GetEnemyAt(y, x);
                        if (enemy != null)
                        {
                            Console.Write(enemy.Symbol);
                        }
                        else if (floor.GetItemAt(y, x) is Item item)
                        {
                            Console.Write(item.Symbol);
                        }
                        else
                        {
                            Console.Write(floor.Map.GetTile(y, x));
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
        
        Console.SetCursorPosition(origin.col, origin.row + floor.Map.Height + 1);
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

    static void ProcessEnemyTurn(Floor floor, Player player, List<string> logs)
    {
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        int[,] dist = BuildDistanceMap(floor.Map, player.Y, player.X);
        
        var orderedEnemies = floor.Enemies.OrderBy(e => dist[e.Y,e.X]).ToList();
        
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
                        
                if (ny < 0 || ny >= floor.Map.Height || nx < 0 || nx >= floor.Map.Width) continue;
                if (dist[ny, nx] == -1) continue;
                if (dist[ny, nx] >= bestDist) continue;
                if (floor.GetEnemyAt(ny, nx) != null) continue;
                if (player.Y == ny && player.X == nx) continue;
                        
                bestDist = dist[ny, nx];
                bestY = ny;
                bestX = nx;
            }
            enemy.MoveTo(bestY, bestX);
        }
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

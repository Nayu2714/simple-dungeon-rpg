using System.Text;

class Program
{
    static readonly int NewLineLength = Environment.NewLine.Length;
    
    static void Main()
    {
        Console.Clear();
        Console.CursorVisible = false;

        string[] mapSource =
        {
            "......#..",
            ".###.....",
            ".#....#..",
            ".##.#....",
            ".#......."
        };

        Map map = new Map(mapSource);
        Player player = new Player(2, 2);
        List<Enemy> enemies = new List<Enemy>();
        enemies.Add(new Enemy("Enemy", 0, 0));
        enemies.Add(new Enemy("Enemy", 1, 4));
        enemies.Add(new Enemy("Enemy", 4, 7));

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
            Draw(map, player, enemies, mapOriginCursor, logs);
            
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
                    else
                    {
                        player.TakeDamage(target.Atk);
                        logs.Add($"{target.Name}の攻撃！ {target.Atk} ダメージ");
                    }
                }
                else if(map.CanMoveTo(nextY, nextX))
                {
                    player.MoveTo(nextY, nextX);
                }
            }
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("ゲームを終了しました。");
    }

    static void Draw(Map map, Player player, List<Enemy> enemies, (int row, int col) origin, List<string> logs)
    {
        //StringBuilder sb = new StringBuilder((map.Width + NewLineLength) * map.Height);
        StringBuilder sb = new StringBuilder();
        
        // --- I. メイン画面（マップ） ---
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (player.Y == y && player.X == x)
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
                    else
                    {
                        sb.Append(map.GetTile(y, x));
                    }
                }
            }
            sb.AppendLine();
        }
        /*
        int playerIndex = player.Y * (map.Width + NewLineLength) + player.X;
        sb[playerIndex] = player.Symbol;

        foreach (Enemy enemy in enemies)
        {
            int enemyIndex = enemy.Y * (map.Width + NewLineLength) + enemy.X;
            sb[enemyIndex] = enemy.Symbol;
        }
        */
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
}

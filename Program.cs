using System;
using System.Linq;
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
            ".....",
            ".###.",
            ".#...",
            ".##.#",
            ".#..."
        };

        Map map = new Map(mapSource);
        Player player = new Player(2, 2);

        Console.WriteLine("【Q】キーを押すと終了します。");
        Console.WriteLine("---------------------------");
        Console.WriteLine();

        // マップ描画用 描画開始位置
        (int row, int col) mapOriginCursor = (Console.CursorTop, Console.CursorLeft);

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        bool isRunning = true;
        while (isRunning)
        {
            Draw(map, player, mapOriginCursor);

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

                if(map.CanMoveTo(nextY, nextX))
                {
                    player.MoveTo(nextY, nextX);
                }
            }
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("ゲームを終了しました。");
    }

    static void Draw(Map map, Player player, (int row, int col) origin)
    {
        StringBuilder sb = new StringBuilder((map.Width + NewLineLength) * map.Height);
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                sb.Append(map.GetTile(y, x));
            }
            sb.AppendLine();
        }

        int index = player.Y * (map.Width + NewLineLength) + player.X;
        sb[index] = '@';
        
        Console.SetCursorPosition(origin.col, origin.row);
        Console.Write(sb.ToString());
    }
}

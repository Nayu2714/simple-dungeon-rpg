using System;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        Console.Clear();

        (int y, int x) playerPos = (2, 2);

        string[] mapSource =
        {
            ".....",
            ".###.",
            ".#...",
            ".##.#",
            ".#..."
        };

        int height = mapSource.Length;
        int width = mapSource[0].Length;

        char[,] map = new char[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                map[y, x] = mapSource[y][x];

        int newlineLength = Environment.NewLine.Length;

        Console.CursorVisible = false;

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
            StringBuilder sb = new StringBuilder((width + newlineLength) * height);
            // string row = new string('.', mapSize.x);
            // for (int i = 0; i < mapSize.y; i++)
            // {
            //     sb.AppendLine(row);
            // }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(map[y, x]);
                }
                sb.AppendLine();
            }
            int playerPosIndex = playerPos.y * (width + newlineLength) + playerPos.x;
            sb[playerPosIndex] = '@';

            Console.SetCursorPosition(mapOriginCursor.col, mapOriginCursor.row);
            Console.Write(sb.ToString());

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
                int nextY = playerPos.y + dy[dir];
                int nextX = playerPos.x + dx[dir];

                if(CanMoveTo(map, nextY, nextX))
                {
                    playerPos = (nextY, nextX);
                }
            }

            if (inputKey.Key == ConsoleKey.Q) isRunning = false;
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("ゲームを終了しました。");
    }

    static bool CanMoveTo(char[,] map, int y, int x)
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);

        if(y >= 0 && y < h &&
           x >= 0 && x < w &&
           map[y, x] == '.')
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

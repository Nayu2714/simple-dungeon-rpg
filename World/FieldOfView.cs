namespace simple_dungeon_rpg.World;

public static class FieldOfView
{
    // 8象限分の座標変換係数: (row, col) => 実座標(y, x) への変換用
    private static readonly int[] Yrow = { 0, 1, 1, 0, 0, -1, -1, 0 };
    private static readonly int[] Xrow = { 1, 0, 0, -1, -1, 0, 0, 1 };
    private static readonly int[] Ycol = { 1, 0, 0, 1, -1, 0, 0, -1 };
    private static readonly int[] Xcol = { 0, 1, -1, 0, 0, -1, 1, 0 };
    
    // 「シャドウキャスト方式FOV」
    // YX空間 を 45度8分割した象限それぞれで CastLight()を実行.
    public static void Compute(Map map, int playerY, int playerX, int radius)
    {
        map.Reveal(playerY, playerX);

        for (int oct = 0; oct < 8; oct++)
        {
            CastLight(map, playerY, playerX, radius, 1, 1.0, 0.0, Yrow[oct], Xrow[oct], Ycol[oct],  Xcol[oct]);
        }
    }

    private static void CastLight(Map map, int playerY, int playerX, int radius, int row, double startSlope, double endSlope, int yrow, int xrow, int ycol, int xcol)
    {
        if (startSlope < endSlope) return;

        bool wasWall = false;

        for (int col = row; col >= 0; col--)
        {
            double highSlope = (col + 0.5) / row; // 遠い方の角 傾き:大
            double lowSlope = (col - 0.5) / row; // 近い方の角 傾き:小
            
            if (lowSlope > startSlope) continue;
            if (highSlope < endSlope) break;

            int trueY = playerY + row * yrow + col * ycol;
            int trueX = playerX + row * xrow + col * xcol;
            if (radius * radius >= row * row + col * col)
            {
                map.Reveal(trueY, trueX);

                bool isWall = !map.CanMoveTo(trueY, trueX);

                if (isWall)
                {
                    if (wasWall == false)
                    {
                        CastLight(map, playerY, playerX, radius, row + 1, startSlope, highSlope, yrow, xrow, ycol, xcol);
                        wasWall = true;
                    }
                }
                else
                {
                    if (wasWall == true)
                    {
                        startSlope = highSlope;
                        wasWall = false;
                    }
                }
            }
        }

        if (wasWall == false && row + 1 <= radius)
        {
            CastLight(map, playerY, playerX,radius, row + 1, startSlope, endSlope, yrow, xrow, ycol, xcol);
        }
    }
}
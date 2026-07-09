namespace simple_dungeon_rpg.World;

public static class FieldOfView
{
    // 8象限分の座標変換係数: ローカル座標(depth, side) => 実座標(y, x) への変換用
    private static readonly int[] Ydepth = { 0, 1, 1, 0, 0, -1, -1, 0 };
    private static readonly int[] Xdepth = { 1, 0, 0, -1, -1, 0, 0, 1 };
    private static readonly int[] Yside = { 1, 0, 0, 1, -1, 0, 0, -1 };
    private static readonly int[] Xside = { 0, 1, -1, 0, 0, -1, 1, 0 };
    
    // 「シャドウキャスト方式FOV」
    // YX空間 を 45度8分割した象限それぞれで CastLight()を実行.
    public static void Compute(Map map, int playerY, int playerX, int radius)
    {
        map.Reveal(playerY, playerX);

        for (int oct = 0; oct < 8; oct++)
        {
            CastLight(map, playerY, playerX, radius, 1, 1.0, 0.0, Ydepth[oct], Xdepth[oct], Yside[oct],  Xside[oct]);
        }
    }

    private static void CastLight(Map map, int playerY, int playerX, int radius, int depth, double startSlope, double endSlope, int ydepth, int xdepth, int yside, int xside)
    {
        if (startSlope < endSlope) return;
        // 担当範囲（startSlope, endSlope）の反転＝空になっている場合の return. 担当外のマスへのReveal()を防ぐ.

        bool wasWall = false;
        
        for (int side = depth; side >= 0; side--) // 傾き: 1 => 0 の方向にReveal()を行っていく.
        {
            // 1*1のマス（厳密には少し違うが）をイメージ.
            double highSlope = (side + 0.5) / depth; // 傾き 大、斜め線（傾き１の直線）に近い方
            double lowSlope = (side - 0.5) / depth; // 傾き 小、奥行き直線（傾き０の直線）に近いほう
            
            if (lowSlope > startSlope) continue;
            // このマスの下端（lowSlope）がまだ斜め線（startSlope）よりも外側 → 担当範囲にまだ入っていないので continue.
            
            if (highSlope < endSlope) break;
            // このマスの上端（highSlope）が既に奥行き直線（endSlope）よりも外側 → 担当範囲を通り過ぎているので break.

            int trueY = playerY + depth * ydepth + side * yside;
            int trueX = playerX + depth * xdepth + side * xside;
            if (radius * radius >= depth * depth + side * side)
            {
                map.Reveal(trueY, trueX);

                bool isWall = !map.CanMoveTo(trueY, trueX);

                if (isWall)
                {
                    if (wasWall == false)
                    {
                        // 壁に当たったら、塗りつぶし範囲を startSlope ~ highSlope にした上で、1マス奥に進んで CastLight().
                        CastLight(map, playerY, playerX, radius, depth + 1, startSlope, highSlope, ydepth, xdepth, yside, xside);
                        wasWall = true;
                    }
                }
                else
                {
                    if (wasWall == true)
                    {
                        // 壁から抜け出した = 奥に進める ので、startSlope = highSlope で、次の CastLight() の起点にする.
                        startSlope = highSlope;
                        wasWall = false;
                    }
                }
            }
        }

        if (wasWall == false && depth + 1 <= radius)
        {
            CastLight(map, playerY, playerX,radius, depth + 1, startSlope, endSlope, ydepth, xdepth, yside, xside);
        }
    }
}
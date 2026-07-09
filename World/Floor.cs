using simple_dungeon_rpg.Entities;
using simple_dungeon_rpg.Items;

namespace simple_dungeon_rpg.World;

public class Floor
{
    public Map Map { get; }
    public List<Enemy> Enemies { get; }
    public List<(Item item, int y, int x)> FloorItems { get; }
    public int Number { get; }

    public Enemy? GetEnemyAt(int y, int x) => GetEnemyAt(Enemies, y, x);
    private static Enemy? GetEnemyAt(List<Enemy> enemies, int y, int x)
    {
        return enemies.FirstOrDefault(enemy => enemy.Y == y && enemy.X == x);
    }
    
    public Item? GetItemAt(int y, int x) => GetItemAt(FloorItems, y, x);
    private static Item? GetItemAt(List<(Item item, int y, int x)> floorItems, int y, int x)
    {
        return floorItems.FirstOrDefault(items => items.y == y && items.x == x).item;
    }
    
    public List<Item> GetItemsAt(int y, int x) => GetItemsAt(FloorItems, y, x);
    private static List<Item> GetItemsAt(List<(Item item, int y, int x)> floorItems, int y, int x)
    {
        return floorItems.Where(entry => entry.y == y && entry.x == x).Select(entry => entry.item).ToList();
    }
    
    private Floor(Map map, List<Enemy> enemies, List<(Item item, int y, int x)> floorItems, int Number)
    {
        Map = map;
        Enemies = enemies;
        FloorItems = floorItems;
        this.Number = Number;
    }

    public static Floor Generate(Random rng, int floorNumber)
    {
        // I. フロアマップ 生成
        Map map = Map.Generate(20, 40, rng);
        
        // II. 階段 配置
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
        
        // III. Enemy 配置
        List<Enemy> enemies = new List<Enemy>();
        int enemyNums = 4;
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

        return new Floor(map, enemies, floorItems, floorNumber);
    }
}
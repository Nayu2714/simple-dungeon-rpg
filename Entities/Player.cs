using simple_dungeon_rpg.Items;

namespace simple_dungeon_rpg.Entities;

public class Player : Entity
{
    public override char Symbol => '@';
    
    public IReadOnlyList<Item> Inventory => _inventory;
    private readonly List<Item> _inventory;

    public Player(int y, int x) : base(y, x, maxHp: 20, hp: 20, atk: 2)
    {
        _inventory = new List<Item>();
    }

    public void AddItem(Item item)
    {
        _inventory.Add(item);
    }
}
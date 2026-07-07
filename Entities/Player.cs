using simple_dungeon_rpg.Items;
using simple_dungeon_rpg.Items.Interfaces;

namespace simple_dungeon_rpg.Entities;

public class Player : Entity
{
    public override int Atk => base.Atk + (Weapon?.Atk ?? 0);
    public int VisionRadius { get; private set; }
    
    public override char Symbol => '@';
    
    public IReadOnlyList<Item> Inventory => _inventory;
    private readonly List<Item> _inventory;
    
    public Weapon? Weapon { get; private set; }

    public Player(int y, int x) : base(y, x, maxHp: 20, hp: 20, atk: 2)
    {
        _inventory = new List<Item>();
        VisionRadius = 6;
    }

    public void Equip(IEquippable equippable)
    {
        if (equippable is Weapon weapon)
        {
            if (Weapon == null)
            {
                this.Weapon = weapon;
                RemoveItem(weapon);
            }
            else
            {
                AddItem(this.Weapon);
                this.Weapon = weapon;
                RemoveItem(weapon);
            }
        }
        // 将来的に 防具・アクセサリー 枠を追加する。
    }

    public void UnEquip()
    {
        AddItem(this.Weapon);
        this.Weapon = null;
    }
    
    public void AddItem(Item item)
    {
        _inventory.Add(item);
    }

    public void RemoveItem(Item item)
    {
        _inventory.Remove(item);
    }
}
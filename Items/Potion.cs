using simple_dungeon_rpg.Entities;
using simple_dungeon_rpg.Items.Interfaces;

namespace simple_dungeon_rpg.Items;

public class Potion : Item, IUsable
{
    public bool IsConsumable => true;

    public override char Symbol => '!';
    
    public int HealAmount { get; private set; }

    public Potion(string name, int healAmount) : base(name)
    {
        this.HealAmount = healAmount;
    }

    public void Use(Entity target)
    {
        target.Heal(HealAmount);
    }
}
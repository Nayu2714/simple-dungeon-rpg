using simple_dungeon_rpg.Items.Interfaces;

namespace simple_dungeon_rpg.Items;

public class Weapon : Item, IEquippable
{
    public override char Symbol => '(';
    
    public int Atk { get; private set; }

    public Weapon(string name, int atk) : base(name)
    {
        this.Atk = atk;
    }
}
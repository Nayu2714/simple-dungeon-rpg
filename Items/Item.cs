namespace simple_dungeon_rpg.Items;

public abstract class Item
{
    public string Name {get; private set;}
    
    public abstract char Symbol { get; }

    protected Item(string name)
    {
        this.Name = name;
    }
}
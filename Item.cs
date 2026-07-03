public abstract class Item
{
    public string Name {get; private set;}

    protected Item(string name)
    {
        this.Name = name;
    }
}
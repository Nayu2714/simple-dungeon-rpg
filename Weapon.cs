public class Weapon : Item, IEquippable
{
    public int Atk { get; private set; }

    public Weapon(string name, int atk) : base(name)
    {
        this.Atk = atk;
    }
}
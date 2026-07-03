public class Potion : Item, IUsable
{
    public bool IsConsumable => true;
    
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
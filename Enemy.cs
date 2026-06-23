public class Enemy : Entity
{
    public override char Symbol => 'E';
    
    public string Name { get; private set; }

    public Enemy(string name, int y, int x) : base(y, x, maxHp: 5, hp: 5, atk: 2)
    {
        this.Name = name;
    }
}
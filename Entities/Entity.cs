namespace simple_dungeon_rpg.Entities;

public abstract class Entity
{
    public int Y { get; private set; }
    public int X { get; private set; }
    
    public int MaxHp { get; private set; }
    public int Hp { get; private set; }
    public int Atk { get; private set; }

    public bool IsDead => Hp <= 0;
    
    public abstract char Symbol { get; }
    
    protected Entity(int y, int x, int maxHp, int hp, int atk)
    {
        this.Y = y;
        this.X = x;
        this.MaxHp = Math.Max(maxHp, 0);
        this.Hp = Math.Clamp(hp, 0, MaxHp);
        this.Atk = Math.Max(atk, 0);
    }
    
    public void MoveTo(int y, int x) { this.Y = y; this.X = x; }

    public void TakeDamage(int amount)
    {
        this.Hp = Math.Clamp(this.Hp - amount, 0, MaxHp);
    }

    public void Heal(int amount)
    {
        this.Hp = Math.Clamp(this.Hp + amount, 0, MaxHp);
    }
}
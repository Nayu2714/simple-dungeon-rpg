public class Player : Entity
{
    public override char Symbol => '@';

    public Player(int y, int x) : base(y, x, maxHp: 20, hp: 20, atk: 2)
    {
        
    }
}
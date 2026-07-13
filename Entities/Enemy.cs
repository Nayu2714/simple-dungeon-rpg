namespace simple_dungeon_rpg.Entities;

public class Enemy : Entity
{
    public string Name { get; private set; }
    public override char Symbol => _symbol;
    private readonly char _symbol;

    public Enemy(EnemyDefinition definition, int y, int x) : base(y, x, maxHp: definition.MaxHp, hp: definition.MaxHp, atk: definition.Atk)
    {
        this.Name = definition.Name;
        this._symbol = definition.Symbol;
    }
}
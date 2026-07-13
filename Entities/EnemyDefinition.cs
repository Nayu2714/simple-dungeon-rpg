namespace simple_dungeon_rpg.Entities;

public class EnemyDefinition
{
    public string Name { get; }
    public char Symbol { get; }
    public int MaxHp { get; }
    public int Atk { get; }

    public EnemyDefinition(string name, char symbol, int maxHp, int atk)
    {
        Name = name;
        Symbol = symbol;
        MaxHp = maxHp;
        Atk = atk;
    }

    public static readonly EnemyDefinition MossRat = new EnemyDefinition("新緑のネズミ", 'R', 3, 1);
    public static readonly EnemyDefinition LoneWolf = new EnemyDefinition("はぐれオオカミ", 'W', 6, 3);
    public static readonly EnemyDefinition RustyAutomaton = new EnemyDefinition("錆鉄のオートマトン", 'A', 12, 5);
}
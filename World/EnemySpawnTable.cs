using simple_dungeon_rpg.Entities;

namespace simple_dungeon_rpg.World;

public class EnemySpawnEntry
{
    public EnemyDefinition Definition { get; }
    public int MinFloor { get; }
    public int MaxFloor { get; }
    public int Weight { get; }
    
    public EnemySpawnEntry(EnemyDefinition definition, int  minFloor, int maxFloor, int weight)
    {
        Definition = definition;
        MinFloor = minFloor;
        MaxFloor = maxFloor;
        Weight = weight;
    }
}

public static class EnemySpawnTable
{
    private static readonly List<EnemySpawnEntry> Entries = new List<EnemySpawnEntry>()
    {
        new EnemySpawnEntry(EnemyDefinition.Moss_Rat, minFloor: 1, maxFloor: 999, weight: 3),
        new EnemySpawnEntry(EnemyDefinition.Lone_Wolf, minFloor: 3, maxFloor: 999, weight: 3),
        new EnemySpawnEntry(EnemyDefinition.Rusty_Automaton, minFloor: 6, maxFloor: 999, weight: 2)
    };
    
    public static EnemyDefinition Choose(Random rng, int floorNumber)
    {
        var currentEntry = Entries.Where(s => s.MinFloor <= floorNumber && floorNumber <= s.MaxFloor).ToList();
        if(currentEntry.Count == 0) throw new InvalidOperationException($"No enemy entries found for floor {floorNumber}");
        
        int weightSum = currentEntry.Select(e => e.Weight).Sum();
        int chosenNum = rng.Next(0, weightSum);
        int cumu = 0;
        foreach (var entry in currentEntry)
        {
            cumu += entry.Weight;
            if(chosenNum < cumu) return entry.Definition;
        }

        return null!;
    }
}
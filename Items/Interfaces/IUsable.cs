using simple_dungeon_rpg.Entities;

namespace simple_dungeon_rpg.Items.Interfaces;

public interface IUsable
{
    bool IsConsumable { get; }

    void Use(Entity target);
}
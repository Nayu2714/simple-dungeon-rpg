public interface IUsable
{
    bool IsConsumable { get; }

    void Use(Entity target);
}
namespace AuthAPI.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        return ((Entity)obj).Id == Id;
    }

    public bool Equals(Entity? other) => Equals(other);

    public static bool operator ==(Entity left, Entity right) => Equals(left, right);

    public static bool operator !=(Entity left, Entity right) => !Equals(left, right);

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

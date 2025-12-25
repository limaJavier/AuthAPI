namespace AuthAPI.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        return ((ValueObject)obj)
            .GetEqualityComponents()
            .SequenceEqual(GetEqualityComponents());
    }

    public bool Equals(ValueObject? other) => Equals(other);

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if(left is null && right is null)
            return true;
        else if(left is null)
            return false;
        
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        if(left is null && right is null)
            return false;
        else if(left is null)
            return true;
        
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
}

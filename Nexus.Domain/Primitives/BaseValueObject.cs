namespace Nexus.Domain.Primitives;

public abstract class BaseValueObject : IEquatable<BaseValueObject>
{
    public abstract IEnumerable<object> GetAtomicValues();

    public bool Equals(BaseValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseValueObject other && ValuesAreEqual(other);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues().Aggregate(0, HashCode.Combine);
    }

    private bool ValuesAreEqual(BaseValueObject other)
    {
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }
}
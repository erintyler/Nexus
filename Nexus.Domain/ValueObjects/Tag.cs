using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.ValueObjects;

public class Tag : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 30;
    
    private Tag(string value, TagType type)
    {
        Value = value;
        Type = type;
    }
    
    public string Value { get; }
    public TagType Type { get; }

    public static Result<Tag> Create(string value, TagType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TagErrors.Empty;
        }

        if (value.Length < MinLength)
        {
            return TagErrors.TooShort;
        }

        if (value.Length > MaxLength)
        {
            return TagErrors.TooLong;
        }

        if (!Enum.IsDefined(type))
        {
            return TagErrors.InvalidType;
        }

        return new Tag(value, type);
    }
    
    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return Type;
    }
}
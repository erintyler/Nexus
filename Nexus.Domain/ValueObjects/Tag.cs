using System.Text.Json.Serialization;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.ValueObjects;

public sealed class Tag : BaseValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 30;
    
    [JsonConstructor]
    internal Tag(TagType type, string value)
    {
        Value = value;
        Type = type;
    }
    
    public string Value { get; }
    public TagType Type { get; }

    public static Result<Tag> Create(TagType type, string value)
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

        return new Tag(type, value);
    }
    
    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return Type;
    }
}
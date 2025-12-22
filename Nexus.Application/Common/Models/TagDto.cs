using Nexus.Domain.Enums;

namespace Nexus.Application.Common.Models;

public record TagDto(TagType Type, string Value)
{
    public static bool TryParse(string? value, out TagDto? tag)
    {
        tag = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Split(':', 2);
        if (parts.Length != 2)
            return false;

        if (!Enum.TryParse<TagType>(parts[0], true, out var tagType))
            return false;

        tag = new TagDto(tagType, parts[1]);
        return true;
    }
};
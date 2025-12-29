using Nexus.Application.Common.Models;
using Nexus.Domain.Enums;

namespace Nexus.Api.IntegrationTests.Utilities;

public static class TestDataGenerator
{
    public static TagDto CreateTagDto(TagType type = TagType.General, string? value = null)
    {
        return new TagDto(type, value ?? $"test-{type.ToString().ToLower()}-{Guid.NewGuid():N}");
    }

    public static List<TagDto> CreateTags(int count = 3)
    {
        var tags = new List<TagDto>();
        var types = Enum.GetValues<TagType>();

        for (int i = 0; i < count; i++)
        {
            var type = types[i % types.Length];
            tags.Add(CreateTagDto(type));
        }

        return tags;
    }
}

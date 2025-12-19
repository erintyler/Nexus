using Nexus.Domain.Enums;

namespace Nexus.Application.Features.Tags.Common.Projections;

public class TagCount
{
    public required string Id { get; init; }
    public int Count { get; set; }
    
    public static string GetId(TagType tagType, string tagValue) => $"{tagType}:{tagValue}";
}
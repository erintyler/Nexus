using Nexus.Domain.Enums;

namespace Nexus.Application.Features.Tags.Common.Projections;

public class TagCount
{
    public required string Id { get; set; }
    public required string TagValue { get; init; }
    public TagType TagType { get; init; }
    public int Count { get; set; }
    
    public static string GetId(string tagValue, TagType tagType) => $"{tagType}:{tagValue}";
}
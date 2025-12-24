using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.Tags.GetTagMigrations;

public record TagMigrationDto(
    TagDto SourceTag,
    TagDto TargetTag,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string LastUpdatedBy,
    DateTimeOffset LastUpdatedAt
    );
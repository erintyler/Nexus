using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTagMigrations;

public record GetTagMigrationsQuery(TagDto? SourceTag = null, TagDto? TargetTag = null) : PaginationRequest;
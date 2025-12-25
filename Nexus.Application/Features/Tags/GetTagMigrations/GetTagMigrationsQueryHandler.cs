using Marten;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Tags.GetTagMigrations;

public class GetTagMigrationsQueryHandler
{
    public static async Task<Result<PagedResult<TagMigrationDto>>> HandleAsync(
        GetTagMigrationsQuery request,
        IQuerySession session,
        CancellationToken cancellationToken = default)
    {
        var query = session.Query<TagMigration>()
            .AsQueryable();

        if (request.SourceTag is not null)
        {
            query = query.Where(tm => tm.SourceTag.Type == request.SourceTag.Type && tm.SourceTag.Value == request.SourceTag.Value);
        }

        if (request.TargetTag is not null)
        {
            query = query.Where(tm => tm.TargetTag.Type == request.TargetTag.Type && tm.TargetTag.Value == request.TargetTag.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return TagMigrationErrors.NoResults;
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(t => new TagMigrationDto(
                new TagDto(t.SourceTag.Type, t.SourceTag.Value),
                new TagDto(t.TargetTag.Type, t.TargetTag.Value),
                t.CreatedBy,
                t.CreatedAt,
                t.LastModifiedBy,
                t.LastModified))
            .ToPagedResultAsync(request, totalCount, cancellationToken);
    }
}
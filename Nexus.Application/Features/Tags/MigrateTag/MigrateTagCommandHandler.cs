using Marten;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;

namespace Nexus.Application.Features.Tags.MigrateTag;

public class MigrateTagCommandHandler(IDocumentSession session, IUserContextService userContextService, ILogger<MigrateTagCommandHandler> logger)
{
    private const int BatchSize = 500;

    public async Task<Result<MigrateTagResponse>> HandleAsync(MigrateTagCommand request, CancellationToken ct)
    {
        var userId = userContextService.GetUserId();
        
        // Step 1: Create and store the TagMigration document
        var migrationResult = TagMigration.Create(
            userId: userId,
            source: new TagData(request.Source.Type, request.Source.Value),
            target: new TagData(request.Target.Type, request.Target.Value)
        );

        if (migrationResult.IsFailure)
            return Result.Failure<MigrateTagResponse>(migrationResult.Errors);

        var migration = migrationResult.Value;
        
        // Store the migration document for future lookups
        session.Store(migration);
        await session.SaveChangesAsync(ct);
        
        logger.LogInformation(
            "Tag migration document created: Id={MigrationId}, Source={Source}, Target={Target}",
            migration.Id,
            $"{migration.SourceTag.Type}:{migration.SourceTag.Value}",
            $"{migration.TargetTag.Type}:{migration.TargetTag.Value}");

        // Step 2: Query all posts that have the source tag
        var affectedPosts = session
            .Query<ImagePostReadModel>()
            .Where(p => p.Tags.Any(t => 
                t.Type == request.Source.Type && 
                t.Value == request.Source.Value))
            .ToAsyncEnumerable(ct);

        var totalProcessed = 0;

        logger.LogInformation("Tag migration started: Source={@Source}, Target={Target}", request.Source, request.Target);

        // Step 3: Create the migration event for bulk updates
        var migrationEvent = migration.CreateMigrationEvent(userId);

        // Step 4: Process in batches using Chunk
        await foreach (var batch in affectedPosts.Chunk(BatchSize).WithCancellation(ct))
        {
            var postIds = batch.Select(p => p.Id).ToList();
            
            await AppendTagMigrationEvents(postIds, migrationEvent, ct);
                
            totalProcessed += postIds.Count;
            
            logger.LogInformation(
                "Tag migration progress: Processed {Count} posts in batch, total: {Total}",
                postIds.Count, 
                totalProcessed);
        }

        logger.LogInformation(
            "Tag migration completed successfully. Total posts migrated: {Total}",
            totalProcessed);
            
        var response = new MigrateTagResponse(
            Success: true,
            Message: "Tag migration completed successfully",
            PostsMigrated: totalProcessed
        );
        
        return Result.Success(response);
    }

    private async Task AppendTagMigrationEvents(
        List<Guid> postIds,
        TagMigratedDomainEvent domainEvent,
        CancellationToken ct)
    {
        foreach (var postId in postIds)
        {
            // Append migration event to each affected ImagePost aggregate
            session.Events.Append(postId, domainEvent);
        }

        // Commit the batch transaction
        await session.SaveChangesAsync(ct);
    }
}



using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;

namespace Nexus.Application.Features.Tags.MigrateTag;

public class MigrateTagCommandHandler(
    ITagMigrationRepository repository,
    IUserContextService userContextService,
    ILogger<MigrateTagCommandHandler> logger)
{
    private const int BatchSize = 500;

    public async Task<Result<MigrateTagResponse>> HandleAsync(MigrateTagCommand request, CancellationToken ct)
    {
        var userId = userContextService.GetUserId();
        
        // Step 1: Check for existing migration
        var existingMigrationCheck = await CheckForExistingMigrationAsync(request, ct);
        if (existingMigrationCheck.IsFailure)
        {
            return Result.Failure<MigrateTagResponse>(existingMigrationCheck.Errors);
        }
        
        // Step 2: Create and store the new migration document
        var migrationResult = await CreateAndStoreMigrationAsync(userId, request, ct);
        if (migrationResult.IsFailure)
        {
            return Result.Failure<MigrateTagResponse>(migrationResult.Errors);
        }
        
        var migration = migrationResult.Value;
        
        // Step 3: Update upstream migrations that point to the source tag
        var upstreamMigrationsUpdated = await UpdateUpstreamMigrationsAsync(request, ct);
        
        // Step 4: Migrate all posts with the source tag
        var totalPostsMigrated = await MigratePostsAsync(migration, userId, request, ct);
        
        logger.LogInformation(
            "Tag migration completed successfully. Total posts migrated: {Total}, Upstream migrations updated: {UpstreamCount}",
            totalPostsMigrated,
            upstreamMigrationsUpdated);
            
        var response = new MigrateTagResponse(
            Success: true,
            Message: "Tag migration completed successfully",
            PostsMigrated: totalPostsMigrated,
            UpstreamMigrationsUpdated: upstreamMigrationsUpdated
        );
        
        return Result.Success(response);
    }

    private async Task<Result> CheckForExistingMigrationAsync(MigrateTagCommand request, CancellationToken ct)
    {
        var existingMigration = await repository.GetMigrationBySourceAsync(
            new TagData(request.Source.Type, request.Source.Value),
            ct);

        if (existingMigration is not null)
        {
            logger.LogWarning("Tag migration already exists for source tag: {@SourceTag}", request.Source);
            return TagMigrationErrors.AlreadyExists;
        }
        
        return Result.Success();
    }

    private async Task<Result<TagMigration>> CreateAndStoreMigrationAsync(
        Guid userId, 
        MigrateTagCommand request, 
        CancellationToken ct)
    {
        var migrationResult = TagMigration.Create(
            userId: userId,
            source: new TagData(request.Source.Type, request.Source.Value),
            target: new TagData(request.Target.Type, request.Target.Value)
        );

        if (migrationResult.IsFailure)
        {
            return Result.Failure<TagMigration>(migrationResult.Errors);
        }

        var migration = migrationResult.Value;
        
        await repository.CreateMigrationAsync(migration, ct);
        
        logger.LogInformation(
            "Tag migration document created: Id={MigrationId}, Source={Source}, Target={Target}",
            migration.Id,
            $"{migration.SourceTag.Type}:{migration.SourceTag.Value}",
            $"{migration.TargetTag.Type}:{migration.TargetTag.Value}");
        
        return Result.Success(migration);
    }

    private async Task<int> UpdateUpstreamMigrationsAsync(MigrateTagCommand request, CancellationToken ct)
    {
        var upstreamMigrations = await repository.GetUpstreamMigrationsAsync(
            new TagData(request.Source.Type, request.Source.Value),
            ct);

        if (upstreamMigrations.Count == 0)
        {
            return 0;
        }

        logger.LogInformation(
            "Found {Count} upstream migrations pointing to source tag. Updating them to point to new target.",
            upstreamMigrations.Count);

        var upstreamMigrationsUpdated = 0;
        var migrationsToDelete = new List<TagMigration>();
        var migrationsToCreate = new List<TagMigration>();

        foreach (var upstreamMigration in upstreamMigrations)
        {
            var updatedMigrationResult = TagMigration.Create(
                userId: Guid.Parse(upstreamMigration.CreatedBy),
                source: upstreamMigration.SourceTag,
                target: new TagData(request.Target.Type, request.Target.Value)
            );

            if (updatedMigrationResult.IsFailure)
            {
                logger.LogError(
                    "Failed to update upstream migration {MigrationId}: {Errors}",
                    upstreamMigration.Id,
                    string.Join(", ", updatedMigrationResult.Errors.Select(e => e.Description)));
                continue;
            }

            migrationsToDelete.Add(upstreamMigration);
            migrationsToCreate.Add(updatedMigrationResult.Value);
            upstreamMigrationsUpdated++;
            
            logger.LogInformation(
                "Updated migration: {@OldSource}→{@OldTarget} is now {@NewSource}→{@NewTarget}",
                upstreamMigration.SourceTag,
                upstreamMigration.TargetTag,
                updatedMigrationResult.Value.SourceTag,
                updatedMigrationResult.Value.TargetTag);
        }

        await repository.UpdateMigrationsAsync(migrationsToDelete, migrationsToCreate, ct);
        
        logger.LogInformation(
            "Successfully updated {Count} upstream migrations",
            upstreamMigrationsUpdated);

        return upstreamMigrationsUpdated;
    }

    private async Task<int> MigratePostsAsync(
        TagMigration migration,
        Guid userId,
        MigrateTagCommand request,
        CancellationToken ct)
    {
        var affectedPosts = repository.GetPostsWithTagAsync(
            new TagData(request.Source.Type, request.Source.Value),
            ct);

        var totalProcessed = 0;

        logger.LogInformation("Tag migration started: Source={@Source}, Target={@Target}", request.Source, request.Target);

        var migrationEvent = migration.CreateMigrationEvent(userId);

        await foreach (var batch in affectedPosts.Chunk(BatchSize).WithCancellation(ct))
        {
            var postIds = batch.Select(p => p.Id).ToList();
            
            await repository.AppendMigrationEventsBatchAsync(postIds, migrationEvent, ct);
                
            totalProcessed += postIds.Count;
            
            logger.LogInformation(
                "Tag migration progress: Processed {Count} posts in batch, total: {Total}",
                postIds.Count, 
                totalProcessed);
        }

        return totalProcessed;
    }
}



namespace Nexus.Application.Features.Tags.MigrateTag;

public record MigrateTagResponse(
    bool Success,
    string Message,
    int PostsMigrated,
    int UpstreamMigrationsUpdated
);


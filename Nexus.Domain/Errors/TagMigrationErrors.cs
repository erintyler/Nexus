using Nexus.Domain.Common;

namespace Nexus.Domain.Errors;

public static class TagMigrationErrors
{
    public static readonly Error UserIdEmpty = new(
        "TagMigration.UserId.Empty",
        ErrorType.BusinessRule,
        "User ID cannot be empty.");

    public static readonly Error AlreadyExists = new(
        "TagMigration.AlreadyExists",
        ErrorType.BusinessRule,
        "A tag migration for the specified source tag already exists.");

    public static readonly Error NoResults = new(
        "TagMigration.NoResults",
        ErrorType.NotFound,
        "No tag migrations found matching the specified criteria.");

    public static readonly Error SourceAndTargetIdentical = new(
        "TagMigration.SourceAndTarget.Identical",
        ErrorType.BusinessRule,
        "The source and target tags cannot be identical.");
}
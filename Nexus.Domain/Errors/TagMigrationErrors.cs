using Nexus.Domain.Common;

namespace Nexus.Domain.Errors;

public static class TagMigrationErrors
{
    public static readonly Error UserIdEmpty = new(
        "TagMigration.UserId.Empty",
        ErrorType.BusinessRule,
        "User ID cannot be empty.");
}
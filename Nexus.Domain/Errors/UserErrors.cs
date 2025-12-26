using Nexus.Domain.Common;

namespace Nexus.Domain.Errors;

public static class UserErrors
{
    public static readonly Error DiscordIdEmpty = new(
        "User.DiscordIdEmpty",
        ErrorType.BusinessRule,
        "Discord ID cannot be empty.");

    public static readonly Error DiscordUsernameEmpty = new(
        "User.DiscordUsernameEmpty",
        ErrorType.BusinessRule,
        "Discord username cannot be empty.");
}

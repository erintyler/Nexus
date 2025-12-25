using Nexus.Domain.Common;

namespace Nexus.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidToken = new(
        "Auth.InvalidToken",
        ErrorType.Unauthorized,
        "The provided access token is invalid or expired.");
    
    public static readonly Error DiscordApiError = new(
        "Auth.DiscordApiError",
        ErrorType.ExternalService,
        "Failed to communicate with Discord API.");
}


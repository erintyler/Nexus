using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandHandler(
    IDiscordApiService discordApiService,
    IJwtTokenService jwtTokenService,
    IUserRepository userRepository,
    ILogger<ExchangeTokenCommandHandler> logger)
{
    public async Task<Result<ExchangeTokenResponse>> HandleAsync(
        ExchangeTokenCommand command,
        CancellationToken ct)
    {
        // Validate the Discord access token
        var discordUser = await discordApiService.ValidateTokenAsync(command.AccessToken, ct);

        if (discordUser == null)
        {
            logger.LogWarning("Failed to validate Discord token");
            return AuthErrors.InvalidToken;
        }

        // Get or create internal user
        var user = await userRepository.GetByDiscordIdAsync(discordUser.Id, ct);
        Guid userId;

        if (user == null)
        {
            // Create new user
            var createUserResult = await userRepository.CreateAsync(discordUser.Id, discordUser.Username, ct);

            if (createUserResult.IsFailure)
            {
                return Result.Failure<ExchangeTokenResponse>(createUserResult.Errors);
            }

            userId = createUserResult.Value;
            logger.LogInformation("Created new user {UserId} for Discord user {DiscordId}", userId, discordUser.Id);
        }
        else
        {
            userId = user.Id;
        }

        // Generate JWT token with internal user ID only
        var jwtResult = jwtTokenService.GenerateToken(userId);

        var response = new ExchangeTokenResponse(
            AccessToken: jwtResult.Token,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            Claims: jwtResult.Claims
        );

        return Result.Success(response);
    }
}


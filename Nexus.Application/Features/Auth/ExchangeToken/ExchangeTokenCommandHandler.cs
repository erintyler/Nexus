using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandHandler(
    IDiscordApiService discordApiService,
    IJwtTokenService jwtTokenService,
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
        
        // Generate JWT token with claims
        var jwtResult = jwtTokenService.GenerateToken(discordUser);
        
        var response = new ExchangeTokenResponse(
            AccessToken: jwtResult.Token,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            Claims: jwtResult.Claims
        );
        
        return Result.Success(response);
    }
}


namespace Nexus.Application.Features.Auth.ExchangeToken;

public record ExchangeTokenResponse(
    string AccessToken, 
    string TokenType, 
    int ExpiresIn,
    Dictionary<string, string> Claims);


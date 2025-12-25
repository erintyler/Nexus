namespace Nexus.Application.Common.Contracts;
public record TokenExchangeResponse
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required Dictionary<string, string> Claims { get; init; }
}

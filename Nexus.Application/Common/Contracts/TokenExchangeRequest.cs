namespace Nexus.Application.Common.Contracts;

public record TokenExchangeRequest
{
    public required string AccessToken { get; init; }
}

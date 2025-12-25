namespace Nexus.Application.Common.Models;

public record JwtTokenResult(
    string Token,
    Dictionary<string, string> Claims);


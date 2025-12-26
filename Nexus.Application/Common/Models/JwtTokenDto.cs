namespace Nexus.Application.Common.Models;

public record JwtTokenDto(
    string Token,
    Dictionary<string, string> Claims);


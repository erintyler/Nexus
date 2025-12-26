using Nexus.Application.Common.Contracts;
using Nexus.Application.Common.Models;

namespace Nexus.Application.Common.Services;

public interface IJwtTokenService
{
    JwtTokenDto GenerateToken(DiscordUser user);
}


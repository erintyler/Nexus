using Nexus.Application.Common.Contracts;
using Nexus.Application.Common.Models;

namespace Nexus.Application.Common.Services;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(DiscordUser user);
}


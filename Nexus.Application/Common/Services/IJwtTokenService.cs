using Nexus.Application.Common.Models;

namespace Nexus.Application.Common.Services;

public interface IJwtTokenService
{
    JwtTokenDto GenerateToken(Guid userId);
}


using System.Security.Claims;

namespace Nexus.Api.Services;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public string GetUserName()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty.ToString() : userId;
    }
}
namespace Nexus.Application.Common.Services;

public interface IUserContextService
{
    string GetUserName();
    Guid GetUserId();
}


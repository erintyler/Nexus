using Marten;
using Nexus.Application.Common.Services;

namespace Nexus.Api.Middleware.Wolverine;

public class MartenUserMiddleware
{
    public void Before(IDocumentSession session, IUserContextService userContextService)
    {
        var userName = userContextService.GetUserName();
        session.LastModifiedBy = userName;
    }
}
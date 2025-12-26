using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Nexus.Frontend.Services;

public class JwtCookieTransform : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No validation needed
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // No validation needed
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            // Get the JWT token from the authentication cookie
            var httpContext = transformContext.HttpContext;

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                // Try to get the JWT token from the authentication properties
                var authenticateResult = await httpContext.AuthenticateAsync();

                if (authenticateResult.Succeeded)
                {
                    var jwtToken = authenticateResult.Properties?.GetTokenValue("access_token");

                    if (!string.IsNullOrEmpty(jwtToken))
                    {
                        // Add the JWT as a Bearer token to the Authorization header
                        transformContext.ProxyRequest.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                    }
                }
            }
        });
    }
}

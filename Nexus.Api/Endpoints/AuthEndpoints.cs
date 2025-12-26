using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
using Nexus.Application.Features.Auth.ExchangeToken;
using Nexus.Domain.Common;
using Wolverine;

namespace Nexus.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .WithDescription("Endpoints for authentication.");

        group.MapPost("/exchange", ExchangeTokenAsync)
            .WithName("ExchangeToken")
            .WithSummary("Exchange Discord OAuth token for JWT")
            .WithDescription("Exchange Discord OAuth token for JWT with user claims")
            .Produces<ExchangeTokenResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ExchangeTokenResponse>, UnauthorizedHttpResult, ProblemHttpResult>> ExchangeTokenAsync(
        [FromBody] ExchangeTokenCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Result<ExchangeTokenResponse>>(command, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        // Check if it's an unauthorized error
        if (result.Errors.Any(e => e.Type == ErrorType.Unauthorized))
        {
            return TypedResults.Unauthorized();
        }

        return result.ToUnprocessableEntityProblem();
    }
}


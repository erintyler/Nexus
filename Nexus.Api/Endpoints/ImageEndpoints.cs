using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Common;
using Wolverine;

namespace Nexus.Api.Endpoints;

public static class CreateImageEndpoint
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapImageEndpoints()
        {
            var images = app.MapGroup("/images");
            images.MapCreateImageEndpoint();
        
            return images;
        }

        public RouteHandlerBuilder MapCreateImageEndpoint()
        {
            return app.MapPost("/", async ([FromBody] CreateImagePostCommand command, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, cancellationToken);

                return result.IsFailure ? Results.BadRequest(result) : Results.Ok(result);
            });
        }
    }
}
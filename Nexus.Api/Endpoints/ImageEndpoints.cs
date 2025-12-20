using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
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
            var images = app.MapGroup("/images")
                .WithTags("Images")
                .WithDescription("Endpoints for managing images.");
                
            images.MapCreateImageEndpoint();
        
            return images;
        }

        public void MapCreateImageEndpoint()
        {
            app.MapPost("/", async Task<Results<CreatedAtRoute<CreateImagePostResponse>, ProblemHttpResult>> ([FromBody] CreateImagePostCommand command, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<CreateImagePostResponse>>(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.CreatedAtRoute(result.Value, "GetImageById", new { id = result.Value.Id });
                } 
                    
                return result.ToUnprocessableEntityProblem();
            }).WithName("CreateImage")
            .WithSummary("Create new image")
            .WithDescription("Creates a new image post with the specified title and tags.")
            .Produces<CreateImagePostResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
            
            app.MapGet("/{id:guid}", () => Results.Ok())
                .WithName("GetImageById")
                .WithSummary("Get image by ID")
                .WithDescription("Retrieves the image post with the specified ID.")
                .Produces<Guid>()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}
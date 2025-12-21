using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Application.Features.ImagePosts.GetImageHistory;
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
            
            app.MapGet("/{id:guid}", async Task<Results<Ok<ImagePostDto>, NotFound>> (Guid id, IMessageBus bus, CancellationToken cancellationToken) =>
                {
                    var query = new GetImagePostQuery(id);
                    var result = await bus.InvokeAsync<Result<ImagePostDto>>(query, cancellationToken);

                    if (result.IsSuccess)
                    {
                        return TypedResults.Ok(result.Value);
                    } 
                    
                    return TypedResults.NotFound();
                })
                .WithName("GetImageById")
                .WithSummary("Get image by ID")
                .WithDescription("Retrieves the image post with the specified ID.")
                .Produces<ImagePostDto>()
                .Produces(StatusCodes.Status404NotFound);
            
            app.MapGet("/{id:guid}/history", async Task<Results<Ok<PagedResult<HistoryDto>>, NotFound>> (
                Guid id,
                DateTimeOffset? dateFrom,
                DateTimeOffset? dateTo,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var query = new GetHistoryQuery(id, dateFrom, dateTo);
                var result = await bus.InvokeAsync<Result<PagedResult<HistoryDto>>>(query, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.Ok(result.Value);
                } 
                
                return TypedResults.NotFound();
            }).WithName("GetImageHistory")
            .WithSummary("Get image history")
            .WithDescription("Retrieves the history of changes for the specified image post.")
            .Produces<PagedResult<HistoryDto>>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
        }
    }
    
    
}
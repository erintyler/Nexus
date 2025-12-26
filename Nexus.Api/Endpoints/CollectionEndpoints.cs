using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
using Nexus.Application.Features.Collections.AddImagePostToCollection;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.Collections.GetCollectionById;
using Nexus.Application.Features.Collections.RemoveImagePostFromCollection;
using Nexus.Domain.Common;
using Wolverine;

namespace Nexus.Api.Endpoints;

public static class CollectionEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapCollectionEndpoints()
        {
            var collections = app.MapGroup("/collections")
                .WithTags("Collections")
                .WithDescription("Endpoints for managing collections of image posts.");

            collections.MapCreateCollectionEndpoint();
            collections.MapGetCollectionByIdEndpoint();
            collections.MapAddImagePostToCollectionEndpoint();
            collections.MapRemoveImagePostFromCollectionEndpoint();

            return collections;
        }

        public void MapCreateCollectionEndpoint()
        {
            app.MapPost("/", async Task<Results<CreatedAtRoute<CreateCollectionResponse>, ProblemHttpResult>> ([FromBody] CreateCollectionCommand command, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<CreateCollectionResponse>>(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.CreatedAtRoute(result.Value, "GetCollectionById", new { id = result.Value.Id });
                }

                return result.ToUnprocessableEntityProblem();
            }).WithName("CreateCollection")
            .WithSummary("Create new collection")
            .WithDescription("Creates a new collection with the specified title.")
            .Produces<CreateCollectionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
        }

        public void MapGetCollectionByIdEndpoint()
        {
            app.MapGet("/{id:guid}", async Task<Results<Ok<CollectionReadModel>, NotFound, ProblemHttpResult>> (
                [FromRoute] Guid id,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCollectionByIdQuery(id);
                var result = await bus.InvokeAsync<Result<CollectionReadModel>>(query, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.Ok(result.Value);
                }

                return TypedResults.NotFound();
            }).WithName("GetCollectionById")
            .WithSummary("Get collection by ID")
            .WithDescription("Retrieves a collection by its unique identifier, including aggregated tags from all child image posts.")
            .Produces<CollectionReadModel>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
        }

        public void MapAddImagePostToCollectionEndpoint()
        {
            app.MapPost("/{id:guid}/images/{imagePostId:guid}", async Task<Results<Ok, ProblemHttpResult>> (
                [FromRoute] Guid id,
                [FromRoute] Guid imagePostId,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var command = new AddImagePostToCollectionCommand(id, imagePostId);
                var result = await bus.InvokeAsync<Result>(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.Ok();
                }

                return result.ToUnprocessableEntityProblem();
            }).WithName("AddImagePostToCollection")
            .WithSummary("Add image post to collection")
            .WithDescription("Adds an existing image post to a collection.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
        }

        public void MapRemoveImagePostFromCollectionEndpoint()
        {
            app.MapDelete("/{id:guid}/images/{imagePostId:guid}", async Task<Results<Ok, ProblemHttpResult>> (
                [FromRoute] Guid id,
                [FromRoute] Guid imagePostId,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var command = new RemoveImagePostFromCollectionCommand(id, imagePostId);
                var result = await bus.InvokeAsync<Result>(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.Ok();
                }

                return result.ToUnprocessableEntityProblem();
            }).WithName("RemoveImagePostFromCollection")
            .WithSummary("Remove image post from collection")
            .WithDescription("Removes an image post from a collection.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
        }
    }
}

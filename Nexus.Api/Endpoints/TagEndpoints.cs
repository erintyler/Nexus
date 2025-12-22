using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.Tags.GetTags;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Common;
using Wolverine;

namespace Nexus.Api.Endpoints;

public static class TagEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTagEndpoints()
        {
            var tags = app.MapGroup("/tags")
                .WithTags("Tags")
                .WithDescription("Endpoints for managing tags.");
                
            tags.MapSearchTagsEndpoint();
            tags.MapMigrateTagEndpoint();
        
            return tags;
        }

        public void MapSearchTagsEndpoint()
        {
            app.MapGet("/search", async Task<Results<Ok<PagedResult<TagCountDto>>, ProblemHttpResult>> (
                string? searchTerm,
                IMessageBus bus,
                int pageNumber = PaginationConstants.DefaultPageNumber,
                int pageSize = PaginationConstants.DefaultPageSize,
                CancellationToken cancellationToken = default) =>
                {
                    var query = new GetTagsQuery(searchTerm)
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };

                    var result = await bus.InvokeAsync<Result<PagedResult<TagCountDto>>>(query, cancellationToken);

                    if (result.IsSuccess)
                    {
                        return TypedResults.Ok(result.Value);
                    }

                    return result.ToUnprocessableEntityProblem();
                })
            .WithName("SearchTags")
            .WithSummary("Search tags")
            .WithDescription("Searches for tags with optional filtering by search term and returns paginated results with tag counts.")
            .Produces<PagedResult<TagCountDto>>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
        }

        public void MapMigrateTagEndpoint()
        {
            app.MapPost("/migrate", async Task<Results<Ok<MigrateTagResponse>, ProblemHttpResult>> (
                [FromBody] MigrateTagCommand command, 
                IMessageBus bus, 
                CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<MigrateTagResponse>>(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return TypedResults.Ok(result.Value);
                } 
                    
                return result.ToUnprocessableEntityProblem();
            })
            .WithName("MigrateTag")
            .WithSummary("Migrate tags across all image posts")
            .WithDescription("Migrates all occurrences of a source tag to a target tag across all image posts. This is a batch operation that processes posts efficiently in batches.")
            .Produces<MigrateTagResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesValidationProblem();
        }
    }
}

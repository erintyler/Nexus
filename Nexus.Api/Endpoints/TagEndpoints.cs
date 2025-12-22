using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Extensions;
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
                
            tags.MapMigrateTagEndpoint();
        
            return tags;
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

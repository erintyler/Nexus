using Microsoft.AspNetCore.Http.HttpResults;
using Nexus.Domain.Common;

namespace Nexus.Api.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        public ValidationProblem ToValidationProblem()
        {
            if (!result.IsFailure)
                throw new InvalidOperationException("Cannot convert successful result to validation problem");

            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description ?? string.Empty }),
                title: "One or more validation errors occurred");
        }
        
        public ProblemHttpResult ToUnprocessableEntityProblem()
        {
            if (!result.IsFailure)
                throw new InvalidOperationException("Cannot convert successful result to problem");

            return TypedResults.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Request cannot be processed",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = result.Errors.ToDictionary(e => e.Code, e => e.Description ?? string.Empty)
                });
        }
    }
}
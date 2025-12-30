using Microsoft.AspNetCore.Http.HttpResults;
using Nexus.Domain.Common;
using IResult = Nexus.Domain.Common.IResult;

namespace Nexus.Api.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        public ValidationProblem ToValidationProblem()
        {
            if (!result.IsFailure)
            {
                throw new InvalidOperationException("Cannot convert successful result to validation problem");
            }

            if (result.Errors.Any(e => e.Type != ErrorType.Validation))
            {
                throw new InvalidOperationException("Result contains non-validation errors");
            }

            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description ?? string.Empty }),
                title: "One or more validation errors occurred");
        }

        public ProblemHttpResult ToProblem()
        {
            // Get most severe status code
            var statusCode = result.Errors.Max(e => e.Type) switch 
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.ExternalService => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError
            };
            
            return TypedResults.Problem(
                statusCode: statusCode,
                title: "An error occurred while processing the request",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = result.Errors.ToDictionary(e => e.Code, e => e.Description ?? string.Empty)
                });
        }
    }
}
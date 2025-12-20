using Nexus.Domain.Common;

namespace Nexus.Api.Filters;

public class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        return result switch
        {
            Result { IsFailure : true } r => Results.ValidationProblem(
                r.Errors.ToDictionary(e => e.Code, e => new[] { e.Description ?? string.Empty }),
                title: "One or more validation errors occurred",
                statusCode: StatusCodes.Status422UnprocessableEntity),
            IValueResult { IsFailure: false } r => Results.Ok(r.GetValue()),
            Result { IsSuccess: true } => Results.Ok(),
            _ => result
        };
    }
}
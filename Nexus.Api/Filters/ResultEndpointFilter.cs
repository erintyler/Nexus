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
            Result { IsFailure: true } r => Results.Problem(
                title: r.Error.Code,
                detail: r.Error.Description,
                statusCode: (int)r.Error.StatusCode),
            IValueResult { IsFailure: false } r => Results.Ok(r.GetValue()),
            Result { IsSuccess: true } => Results.Ok(),
            _ => result
        };
    }
}
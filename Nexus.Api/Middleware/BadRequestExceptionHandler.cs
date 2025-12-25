using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Api.Middleware;

public class BadRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            return false;
        }

        var friendlyMessage = badHttpRequestException.Message switch
        {
            var msg when msg.StartsWith("Failed to bind parameter") => "The request contains invalid query parameters.",
            _ => badHttpRequestException.Message
        };

        var problemDetails = new ProblemDetails
        {
            Title = "Bad Request",
            Detail = friendlyMessage,
            Status = StatusCodes.Status400BadRequest
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails, cancellationToken: cancellationToken);

        return true;
    }
}
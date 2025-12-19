using System.Net;

namespace Nexus.Domain.Common;

public sealed record Error(string Code, string? Description = null, HttpStatusCode StatusCode = HttpStatusCode.BadRequest)
{
    public static readonly Error None = new(string.Empty);
    
    public static implicit operator Result(Error error) => Result.Failure(error);
}
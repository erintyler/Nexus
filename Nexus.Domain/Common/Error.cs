using System.Net;

namespace Nexus.Domain.Common;

public sealed record Error(string Code, ErrorType Type, string? Description = null)
{
    public static readonly Error None = new(string.Empty, ErrorType.None);
    
    public static implicit operator Result(Error error) => Result.Failure(error);
}
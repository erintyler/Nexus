using System.Diagnostics.CodeAnalysis;

namespace Nexus.Domain.Common;

public interface IResult
{
    bool IsSuccess { get; }
    
    [MemberNotNullWhen(true, nameof(Error))]
    bool IsFailure => !IsSuccess;
    Error? Error { get; }
}
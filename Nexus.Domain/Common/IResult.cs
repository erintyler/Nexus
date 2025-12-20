using System.Diagnostics.CodeAnalysis;

namespace Nexus.Domain.Common;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    IReadOnlyList<Error> Errors { get; }
}
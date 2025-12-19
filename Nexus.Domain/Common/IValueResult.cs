namespace Nexus.Domain.Common;

public interface IValueResult : IResult
{
    object? GetValue();
}
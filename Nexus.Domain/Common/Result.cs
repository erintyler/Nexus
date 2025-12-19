using System.Diagnostics.CodeAnalysis;

namespace Nexus.Domain.Common;

public class Result : IResult
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error state for Result", nameof(error));
        }
        
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public bool IsSuccess { get; }
    
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public sealed class Result<T> : Result, IValueResult
{
    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        if (isSuccess && value is null)
        {
            throw new ArgumentNullException(nameof(value), "Successful Result must have a value.");
        }
        
        Value = value!;
    }
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
    
    public T Value => IsSuccess 
        ? field
        : throw new InvalidOperationException("Cannot access the value of a failed Result.");

    public object? GetValue()
    {
        return IsSuccess ? Value : null;
    }
}
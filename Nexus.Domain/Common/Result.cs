using System.Diagnostics.CodeAnalysis;

namespace Nexus.Domain.Common;

public class Result : IResult
{
    private readonly List<Error> _errors = [];
    
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error state for Result", nameof(error));
        }
        
        IsSuccess = isSuccess;
        _errors.Add(error);
    }
    
    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        var validErrors = errors.Where(e => e != Error.None).ToList();
        
        if (isSuccess || !isSuccess && validErrors.Count == 0)
        {
            throw new ArgumentException("Invalid error state for Result", nameof(errors));
        }
        
        IsSuccess = isSuccess;
        _errors.AddRange(validErrors);
    }
    
    public bool IsSuccess { get; }
    
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();
    
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(default, false, errors);
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
    
    internal Result(T? value, bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors)
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
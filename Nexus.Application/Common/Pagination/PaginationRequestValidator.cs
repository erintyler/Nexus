using FluentValidation;

namespace Nexus.Application.Common.Pagination;

public abstract class PaginationRequestValidator<T> : AbstractValidator<T> where T : PaginationRequest
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(PaginationConstants.DefaultPageNumber)
            .WithMessage($"Page number must be greater than {PaginationConstants.DefaultPageNumber}.");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize)
            .WithMessage($"Page size must be between {PaginationConstants.MinPageSize} and {PaginationConstants.MaxPageSize}.");
    }
}
using FluentValidation;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public class CreateImagePostCommandValidator : AbstractValidator<CreateImagePostCommand>
{
    public CreateImagePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .Length(5, 200)
            .WithMessage("Title must be between 5 and 100 characters long.");
        
        // Ensure no duplicate tags are provided
        RuleFor(x => x.Tags)
            .Must(tags => tags.Count == tags.Distinct().Count())
            .WithMessage("Duplicate tags are not allowed.");
    }
}
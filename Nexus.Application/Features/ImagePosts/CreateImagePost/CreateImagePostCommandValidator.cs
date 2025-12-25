using FluentValidation;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public class CreateImagePostCommandValidator : AbstractValidator<CreateImagePostCommand>
{
    public CreateImagePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .Length(5, 200)
            .WithMessage("Title must be between 5 and 200 characters long.");

        // Ensure no duplicate tags are provided
        RuleFor(x => x.Tags)
            .Must(tags => tags.Count == tags.Distinct().Count())
            .WithMessage("Duplicate tags are not allowed.");

        RuleFor(x => x.ContentType)
            .Must(BeAnImageContentType)
            .WithMessage("Content type must be a valid image format (jpeg, jpg, png, gif, webp).");
    }

    private static bool BeAnImageContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        return allowedTypes.Contains(contentType.ToLowerInvariant());
    }
}
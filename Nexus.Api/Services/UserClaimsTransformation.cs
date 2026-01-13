using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Constants;

namespace Nexus.Api.Services;

/// <summary>
/// Transforms claims by fetching additional user information from the user repository.
/// This implementation uses a cached repository to ensure high performance.
/// </summary>
public class UserClaimsTransformation(IUserRepository userRepository) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if transformation has already been applied to avoid duplicate calls
        if (principal.HasClaim(c => c.Type == ClaimConstants.Transformed))
        {
            return principal;
        }

        // Get the user ID from the existing claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return principal;
        }

        // Fetch user from repository (cached)
        // Note: Using default cancellation token as IClaimsTransformation doesn't support CancellationToken parameter
        // In production, this should be fast due to caching layer
        var user = await userRepository.GetByIdAsync(userId, default);
        if (user is null)
        {
            return principal;
        }

        // Create a new identity with additional claims
        var identity = new ClaimsIdentity(principal.Identity);

        // Add Discord-related claims
        identity.AddClaim(new Claim(ClaimConstants.DiscordId, user.DiscordId));
        identity.AddClaim(new Claim(ClaimConstants.DiscordUsername, user.DiscordUsername));

        // Add permission claims
        identity.AddClaim(new Claim(ClaimConstants.CanCreateImage, user.CanCreateImage.ToString()));
        identity.AddClaim(new Claim(ClaimConstants.CanEditImage, user.CanEditImage.ToString()));
        identity.AddClaim(new Claim(ClaimConstants.CanAddComment, user.CanAddComment.ToString()));
        identity.AddClaim(new Claim(ClaimConstants.CanAddTags, user.CanAddTags.ToString()));
        identity.AddClaim(new Claim(ClaimConstants.CanDeleteContent, user.CanDeleteContent.ToString()));
        identity.AddClaim(new Claim(ClaimConstants.IsAdmin, user.IsAdmin.ToString()));

        // Add a marker claim to indicate transformation has been applied
        identity.AddClaim(new Claim(ClaimConstants.Transformed, "true"));

        return new ClaimsPrincipal(identity);
    }
}

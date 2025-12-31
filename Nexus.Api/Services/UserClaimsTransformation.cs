using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Nexus.Application.Common.Abstractions;

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
        if (principal.HasClaim(c => c.Type == "transformed"))
        {
            return principal;
        }

        // Get the user ID from the existing claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return principal;
        }

        // Fetch user from repository (cached)
        var user = await userRepository.GetByIdAsync(userId, CancellationToken.None);
        if (user == null)
        {
            return principal;
        }

        // Create a new identity with additional claims
        var identity = new ClaimsIdentity(principal.Identity);

        // Add Discord-related claims
        identity.AddClaim(new Claim("discord_id", user.DiscordId));
        identity.AddClaim(new Claim("discord_username", user.DiscordUsername));

        // Add a marker claim to indicate transformation has been applied
        identity.AddClaim(new Claim("transformed", "true"));

        return new ClaimsPrincipal(identity);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexus.Api.Configuration.Models;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;

namespace Nexus.Api.Services;


public class JwtTokenService(IOptions<JwtSettings> jwtSettings) : IJwtTokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public JwtTokenDto GenerateToken(Guid userId)
    {
        // Note: This JWT now only includes the internal user ID as the NameIdentifier claim.
        // Downstream services are responsible for transforming this user ID into any additional
        // claims they require (for example, Discord-specific claims). This is a breaking change
        // from the previous implementation, which emitted multiple Discord claims directly.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var claimsDictionary = claims.ToDictionary(
            c => c.Type,
            c => c.Value
        );

        return new JwtTokenDto(jwtToken, claimsDictionary);
    }
}


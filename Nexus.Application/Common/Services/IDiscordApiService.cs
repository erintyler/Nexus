using Nexus.Application.Common.Contracts;

namespace Nexus.Application.Common.Services;

public interface IDiscordApiService
{
    Task<DiscordUser?> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}

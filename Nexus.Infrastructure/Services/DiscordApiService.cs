using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Contracts;
using Nexus.Application.Common.Services;

namespace Nexus.Infrastructure.Services;

public class DiscordApiService(
    HttpClient httpClient,
    ILogger<DiscordApiService> logger) : IDiscordApiService
{
    private const string DiscordApiBaseUrl = "https://discord.com/api";
    
    public async Task<DiscordUser?> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync($"{DiscordApiBaseUrl}/users/@me", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to validate Discord token: {StatusCode}", response.StatusCode);
                return null;
            }

            var discordUser = await response.Content.ReadFromJsonAsync<DiscordUser>(cancellationToken: cancellationToken);
            return discordUser;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Discord token");
            return null;
        }
    }
}


using Nexus.Application.Common.Contracts;

namespace Nexus.Frontend.Services;

public class TokenExchangeService(HttpClient httpClient, ILogger<TokenExchangeService> logger) : ITokenExchangeService
{
    public async Task<TokenExchangeResponse?> ExchangeTokenAsync(string accessToken)
    {
        try
        {
            var request = new TokenExchangeRequest { AccessToken = accessToken };

            var response = await httpClient.PostAsJsonAsync("api/auth/exchange", request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Token exchange failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenExchangeResponse>();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exchanging token");
            return null;
        }
    }
}

using Nexus.Application.Common.Contracts;

namespace Nexus.Frontend.Services;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenExchangeService> _logger;

    public TokenExchangeService(
        HttpClient httpClient, 
        ILogger<TokenExchangeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TokenExchangeResponse?> ExchangeTokenAsync(string accessToken)
    {
        try
        {
            var request = new TokenExchangeRequest { AccessToken = accessToken };

            var response = await _httpClient.PostAsJsonAsync("api/auth/exchange", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenExchangeResponse>();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging token");
            return null;
        }
    }
}

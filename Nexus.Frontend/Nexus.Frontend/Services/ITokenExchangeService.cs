using Nexus.Application.Common.Contracts;

namespace Nexus.Frontend.Services;

public interface ITokenExchangeService
{
    Task<TokenExchangeResponse?> ExchangeTokenAsync(string accessToken);
}


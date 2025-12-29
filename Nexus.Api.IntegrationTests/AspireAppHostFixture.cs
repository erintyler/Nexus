using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Xunit;

namespace Nexus.Api.IntegrationTests;

public sealed class AspireAppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _httpClient;

    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("HttpClient not initialized");

    public async ValueTask InitializeAsync()
    {
        var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Nexus_AppHost>();

        _app = await appHostBuilder.BuildAsync();
        await _app.StartAsync();

        _httpClient = _app.CreateHttpClient("nexus-api");
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}

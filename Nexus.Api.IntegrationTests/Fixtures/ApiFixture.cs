using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Nexus.Api.IntegrationTests.Fixtures;

public sealed class ApiFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _httpClient;

    public HttpClient HttpClient
    {
        get
        {
            if (_httpClient == null)
            {
                throw new InvalidOperationException("HttpClient is not initialized. Call InitializeAsync first.");
            }
            return _httpClient;
        }
    }

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Get HttpClient for the API
        _httpClient = _app.CreateHttpClient("nexus-api");
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}

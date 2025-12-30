using Alba;
using JasperFx.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Testcontainers.RabbitMq;

namespace Nexus.Api.IntegrationTests.Fixtures;

public sealed class AlbaWebApplicationFixture : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public IAlbaHost AlbaHost { get; private set; } = null!;

    public AlbaWebApplicationFixture()
    {
        _postgresFixture = new PostgresContainerFixture();
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.0-management-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        // Start containers
        await _postgresFixture.InitializeAsync();
        await _rabbitMqContainer.StartAsync();

        // Set environment variables before creating the host
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer.GetConnectionString());

        // Create Alba host using Program directly
        AlbaHost = await Alba.AlbaHost.For<Program>(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }

    public async ValueTask DisposeAsync()
    {
        AlbaHost.SafeDispose();

        await _rabbitMqContainer.DisposeAsync();
        await _postgresFixture.DisposeAsync();
    }
}

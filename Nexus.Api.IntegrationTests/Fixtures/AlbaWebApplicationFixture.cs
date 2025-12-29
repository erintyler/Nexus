using Alba;
using Alba.Security;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Testcontainers.RabbitMq;
using Xunit;

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

        // Create Alba host using Program directly
        AlbaHost = await Alba.AlbaHost.For<Program>(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Override connection strings to use test containers
                services.AddNpgsqlDataSource(_postgresFixture.ConnectionString);
            });

            builder.UseSetting("ConnectionStrings:postgres", _postgresFixture.ConnectionString);
            builder.UseSetting("ConnectionStrings:rabbitmq", _rabbitMqContainer.GetConnectionString());
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (AlbaHost != null)
        {
            await AlbaHost.DisposeAsync();
        }

        await _rabbitMqContainer.DisposeAsync();
        await _postgresFixture.DisposeAsync();
    }
}

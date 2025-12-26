using Alba;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.Common.Projections;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Projections;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Application.Features.Users.Common.Projections;
using Nexus.Domain.Entities;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Testcontainers.RabbitMq;
using Wolverine.Marten;
using Xunit;

namespace Nexus.Api.IntegrationTests.Fixtures;

public sealed class AlbaWebAppFixture : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private IAlbaHost? _host;

    public AlbaWebAppFixture()
    {
        _postgresFixture = new PostgresContainerFixture();
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.0-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public IAlbaHost Host => _host ?? throw new InvalidOperationException("Host not initialized");

    public async ValueTask InitializeAsync()
    {
        await _postgresFixture.InitializeAsync();
        await _rabbitMqContainer.StartAsync();

        // Set environment variables before building Alba host
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer.GetConnectionString());

        // Build Alba host from Program.cs
        _host = await AlbaHost.For<Program>();

        // Initialize database schema for Marten
        var store = _host.Services.GetRequiredService<IDocumentStore>();
        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            await _host.DisposeAsync();
        }

        await _rabbitMqContainer.DisposeAsync();
        await _postgresFixture.DisposeAsync();
    }
}

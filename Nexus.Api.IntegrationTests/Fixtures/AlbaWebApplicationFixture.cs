using Alba;
using Alba.Security;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Npgsql;
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

        // Apply database migrations
        await ApplyDatabaseMigrationsAsync();

        // Set environment variables before creating the host
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer.GetConnectionString());

        // Create Alba host using Program directly
        AlbaHost = await Alba.AlbaHost.For<Program>(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }

    private async Task ApplyDatabaseMigrationsAsync()
    {
        // Read and execute the migration script
        var migrationScriptPath = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName,
            "Nexus.Migrations",
            "Scripts",
            "up",
            "20251223091846_initial.sql"
        );

        if (!File.Exists(migrationScriptPath))
        {
            throw new FileNotFoundException($"Migration script not found at {migrationScriptPath}");
        }

        var migrationSql = await File.ReadAllTextAsync(migrationScriptPath);

        await using var connection = new NpgsqlConnection(_postgresFixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = migrationSql;
        await command.ExecuteNonQueryAsync();
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

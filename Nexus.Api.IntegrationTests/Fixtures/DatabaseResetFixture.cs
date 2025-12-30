using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Api.IntegrationTests.Fixtures;

/// <summary>
/// Base test class that resets the database before each test run.
/// All integration tests should inherit from this class to ensure test isolation.
/// </summary>
public abstract class DatabaseResetFixture : IClassFixture<AlbaWebApplicationFixture>, IAsyncLifetime
{
    protected readonly AlbaWebApplicationFixture Fixture;

    protected DatabaseResetFixture(AlbaWebApplicationFixture fixture)
    {
        Fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        // Reset the database before each test
        var store = Fixture.AlbaHost.Services.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.CompletelyRemoveAllAsync();
        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        // No cleanup needed after each test
        return ValueTask.CompletedTask;
    }
}


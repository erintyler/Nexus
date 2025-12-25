using grate.DependencyInjection;
using grate.Migration;
using grate.postgresql.DependencyInjection;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Projections;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Domain.Entities;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Wolverine.Marten;

namespace Nexus.Migrations.IntegrationTests;

public class MigrationTests : IClassFixture<PostgresContainerFixture>
{
    private readonly IGrateMigrator _grateMigrator;
    private readonly IDocumentStore _documentStore;
    private readonly PostgresContainerFixture _fixture;

    public MigrationTests(PostgresContainerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(b => b.AddXUnit(output));
        serviceCollection.AddNpgsqlDataSource(_fixture.ConnectionString);
        serviceCollection.AddGrate(o =>
        {
            o.WithConnectionString(_fixture.ConnectionString);
            o.WithSqlFilesDirectory("Scripts");
        }).UsePostgreSQL();

        // TODO: Reuse Marten configuration from API project
        serviceCollection.AddMarten(o =>
            {
                o.Projections.Add<ImagePostProjection>(ProjectionLifecycle.Inline);
                o.Projections.Add<TagCountProjection>(ProjectionLifecycle.Async);

                o.OpenTelemetry.TrackConnections = TrackLevel.Normal;
                o.OpenTelemetry.TrackEventCounters();

                o.Events.MetadataConfig.UserNameEnabled = true;
                o.Policies.ForAllDocuments(x => { x.Metadata.CreatedAt.Enabled = true; });

                o.Schema.For<TagCount>().Identity(x => x.Id);

                // Configure TagMigration document with index on source tag for fast lookups
                o.Schema.For<TagMigration>()
                    .Index(x => x.SourceTag.Type)
                    .Index(x => x.SourceTag.Value);

                o.Schema.For<ImagePostReadModel>().Metadata(m =>
                {
                    m.CreatedAt.MapTo(x => x.CreatedAt);
                    m.LastModified.MapTo(x => x.LastModified);
                    m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
                });
            })
            .UseNpgsqlDataSource()
            .IntegrateWithWolverine()
            .AddAsyncDaemon(DaemonMode.HotCold);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _documentStore = serviceProvider.GetRequiredService<IDocumentStore>();
        _grateMigrator = serviceProvider.GetRequiredService<IGrateMigrator>();
    }

    [Fact]
    public async Task Migrations_AreAppliedSuccessfully_And_ThereAreNoPendingMigrations()
    {
        // Act
        await _grateMigrator.Migrate();

        // Assert
        await _documentStore.Storage.Database.AssertDatabaseMatchesConfigurationAsync(TestContext.Current.CancellationToken);
    }
}
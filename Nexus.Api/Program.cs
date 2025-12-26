using System.Text.Json.Serialization;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Microsoft.AspNetCore.Authentication;
using Nexus.Api.Configuration.Models;
using Nexus.Api.Endpoints;
using Nexus.Api.Extensions;
using Nexus.Api.Middleware;
using Nexus.Api.Middleware.Wolverine;
using Nexus.Api.Services;
using Nexus.Application.Common.Services;
using Nexus.Application.Configuration.Models;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.Common.Projections;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Projections;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Application.Features.Users.Common.Projections;
using Nexus.Application.Helpers;
using Nexus.Domain.Entities;
using Nexus.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults only when not in test environment
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.AddServiceDefaults();
    builder.AddNpgsqlDataSource("postgres", configureDataSourceBuilder: o =>
    {
        o.ConfigureTracing(c =>
        {
            c.ConfigureCommandFilter(f => !f.CommandText.Contains("HighWaterMark") && !f.CommandText.Contains("wolverine_control_queue"));
        });
    });
}
else
{
    // For tests, add NpgsqlDataSource with connection string from configuration
    builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("postgres") ?? throw new InvalidOperationException("Postgres connection string not found"));
}

builder.UseWolverine(o =>
{
    o.Discovery.IncludeAssembly(typeof(CreateImagePostCommandHandler).Assembly);

    o.Policies.AutoApplyTransactions();
    o.Policies.AddMiddleware(typeof(MartenUserMiddleware));
    o.UseFluentValidation();

    // Only configure RabbitMQ and agents in non-test environments
    if (!builder.Environment.IsEnvironment("Test"))
    {
        o.UseRabbitMqUsingNamedConnection("rabbitmq")
            .AutoProvision();

        o.PublishMessage<ProcessImageCommand>()
            .ToRabbitQueue("image-processing");
    }
    else
    {
        // Disable external transports for tests only
        o.Services.DisableAllExternalWolverineTransports();
    }

    o.Services.CritterStackDefaults(c =>
    {
        if (builder.Environment.IsEnvironment("Test"))
        {
            c.Production.GeneratedCodeMode = TypeLoadMode.Auto;
            c.Production.ResourceAutoCreate = AutoCreate.CreateOrUpdate; // Allow schema creation in tests
        }
        else
        {
            c.Production.GeneratedCodeMode = TypeLoadMode.Static;
            c.Production.ResourceAutoCreate = AutoCreate.None;
        }
    });
});

if (CodeGeneration.IsRunningGeneration())
{
    builder.Services.DisableAllExternalWolverineTransports();
    builder.Services.DisableAllWolverineMessagePersistence();
}

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (CodeGeneration.IsRunningGeneration())
{
    builder.Services
        .AddMarten("Server=.;Database=Foo")
        .AddAsyncDaemon(DaemonMode.Disabled)
        .UseLightweightSessions();
}
else if (builder.Environment.IsEnvironment("Test"))
{
    // Test environment configuration
    builder.Services.AddMarten(o =>
        {
            o.Projections.Add<ImagePostProjection>(ProjectionLifecycle.Inline);
            o.Projections.Add<CollectionProjection>(ProjectionLifecycle.Inline);
            o.Projections.Add<TagCountProjection>(ProjectionLifecycle.Inline); // Inline for tests
            o.Projections.Add<UserProjection>(ProjectionLifecycle.Inline);

            o.Events.MetadataConfig.UserNameEnabled = true;
            o.Policies.ForAllDocuments(x => { x.Metadata.CreatedAt.Enabled = true; });

            o.Schema.For<TagCount>().Identity(x => x.Id);

            // Configure TagMigration document with index on source tag for fast lookups
            o.Schema.For<TagMigration>()
                .Index(x => x.SourceTag.Type)
                .Index(x => x.SourceTag.Value)
                .Metadata(m =>
                {
                    m.CreatedAt.MapTo(x => x.CreatedAt);
                    m.LastModified.MapTo(x => x.LastModified);
                    m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
                });

            // Configure User document with index on DiscordId for fast lookups
            o.Schema.For<User>()
                .Index(x => x.DiscordId)
                .Metadata(m =>
                {
                    m.CreatedAt.MapTo(x => x.CreatedAt);
                    m.LastModified.MapTo(x => x.LastModified);
                    m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
                });

            o.Schema.For<ImagePostReadModel>().Metadata(m =>
            {
                m.CreatedAt.MapTo(x => x.CreatedAt);
                m.LastModified.MapTo(x => x.LastModified);
                m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
            });

            o.Schema.For<CollectionReadModel>().Metadata(m =>
            {
                m.CreatedAt.MapTo(x => x.CreatedAt);
                m.LastModified.MapTo(x => x.LastModified);
                m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
            });
        })
        .UseNpgsqlDataSource()
        .IntegrateWithWolverine()
        .AddAsyncDaemon(DaemonMode.Disabled); // Disabled for tests
}
else
{
    builder.Services.AddMarten(o =>
        {
            o.Projections.Add<ImagePostProjection>(ProjectionLifecycle.Inline);
            o.Projections.Add<CollectionProjection>(ProjectionLifecycle.Inline);
            o.Projections.Add<TagCountProjection>(ProjectionLifecycle.Async);
            o.Projections.Add<UserProjection>(ProjectionLifecycle.Inline);

            o.OpenTelemetry.TrackConnections = TrackLevel.Normal;
            o.OpenTelemetry.TrackEventCounters();

            o.Events.MetadataConfig.UserNameEnabled = true;
            o.Policies.ForAllDocuments(x => { x.Metadata.CreatedAt.Enabled = true; });

            o.Schema.For<TagCount>().Identity(x => x.Id);

            // Configure TagMigration document with index on source tag for fast lookups
            o.Schema.For<TagMigration>()
                .Index(x => x.SourceTag.Type)
                .Index(x => x.SourceTag.Value)
                .Metadata(m =>
                {
                    m.CreatedAt.MapTo(x => x.CreatedAt);
                    m.LastModified.MapTo(x => x.LastModified);
                    m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
                });

            // Configure User document with index on DiscordId for fast lookups
            o.Schema.For<User>()
                .Index(x => x.DiscordId)
                .Metadata(m =>
                {
                    m.CreatedAt.MapTo(x => x.CreatedAt);
                    m.LastModified.MapTo(x => x.LastModified);
                    m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
                });

            o.Schema.For<ImagePostReadModel>().Metadata(m =>
            {
                m.CreatedAt.MapTo(x => x.CreatedAt);
                m.LastModified.MapTo(x => x.LastModified);
                m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
            });

            o.Schema.For<CollectionReadModel>().Metadata(m =>
            {
                m.CreatedAt.MapTo(x => x.CreatedAt);
                m.LastModified.MapTo(x => x.LastModified);
                m.LastModifiedBy.MapTo(x => x.LastModifiedBy);
            });
        })
        .UseNpgsqlDataSource()
        .IntegrateWithWolverine()
        .AddAsyncDaemon(DaemonMode.HotCold);
}

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ImageOptions>(builder.Configuration.GetSection(nameof(ImageOptions)));
builder.Services.AddSingleton<IImageConversionService, ImageConversionService>();
builder.Services.AddSingleton<IThumbnailService, ThumbnailService>();
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITagMigrationService, TagMigrationService>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication("Test") // Set default scheme name
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

// Map default endpoints only when not in test environment
if (!app.Environment.IsEnvironment("Test"))
{
    app.MapDefaultEndpoints();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

api.MapImageEndpoints();
api.MapTagEndpoints();
api.MapCollectionEndpoints();
api.MapAuthEndpoints();


await app.RunJasperFxCommands(args);

// Make Program class accessible to integration tests
public partial class Program { }
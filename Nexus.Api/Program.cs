using System.Text.Json.Serialization;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Microsoft.AspNetCore.Authentication;
using Nexus.Api.Endpoints;
using Nexus.Api.Extensions;
using Nexus.Api.Middleware;
using Nexus.Api.Middleware.Wolverine;
using Nexus.Api.Services;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Projections;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Application.Features.Tags.GetTags;
using Nexus.Application.Helpers;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("postgres", configureDataSourceBuilder: o =>
{
    o.ConfigureTracing(c =>
    {
        c.ConfigureCommandFilter(f => !f.CommandText.Contains("HighWaterMark"));
    });
});
builder.UseWolverine(o =>
{
    o.Discovery.IncludeAssembly(typeof(CreateImagePostCommandHandler).Assembly);
    o.Durability.Mode = DurabilityMode.MediatorOnly;
    
    o.Policies.AutoApplyTransactions();
    o.Policies.AddMiddleware(typeof(MartenUserMiddleware));
    o.UseFluentValidation();

    o.Services.CritterStackDefaults(c =>
    {
        c.Production.GeneratedCodeMode = TypeLoadMode.Static;
        c.Production.ResourceAutoCreate = AutoCreate.None;
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
else
{
    builder.Services.AddMarten(o =>
        {
            o.Projections.Add<ImagePostProjection>(ProjectionLifecycle.Inline);
            o.Projections.Add<TagCountProjection>(ProjectionLifecycle.Async);
            
            o.OpenTelemetry.TrackConnections = TrackLevel.Normal;
            o.OpenTelemetry.TrackEventCounters();
            
            o.Events.MetadataConfig.UserNameEnabled = true;
            o.Policies.ForAllDocuments(x =>
            {
                x.Metadata.CreatedAt.Enabled = true;
            });
            
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
}

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITagMigrationService, TagMigrationService>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication("Test") // Set default scheme name
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

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

api.MapPostToWolverine<GetTagsQuery, Result<PagedResult<TagCount>>>("/tags/search");

await app.RunJasperFxCommands(args);
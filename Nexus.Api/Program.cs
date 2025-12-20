using System.Text.Json.Serialization;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Nexus.Api.Endpoints;
using Nexus.Api.Filters;
using Nexus.Api.Middleware;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Application.Features.Tags.GetTags;
using Nexus.Application.Helpers;
using Nexus.Domain.Common;
using Scalar.AspNetCore;
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
            o.Projections.Add<TagCountProjection>(ProjectionLifecycle.Async);
            o.OpenTelemetry.TrackConnections = TrackLevel.Normal;
            o.OpenTelemetry.TrackEventCounters();
            o.Schema.For<TagCount>().Identity(x => x.Id);
        })
        .UseNpgsqlDataSource()
        .IntegrateWithWolverine()
        .AddAsyncDaemon(DaemonMode.HotCold);
}

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var api = app.MapGroup("/api")
    .AddEndpointFilter<ResultEndpointFilter>();

api.MapImageEndpoints();

api.MapPostToWolverine<GetTagsQuery, Result<PagedResult<TagCount>>>("/tags/search");

await app.RunJasperFxCommands(args);
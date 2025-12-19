using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Filters;
using Nexus.Api.Middleware;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Application.Features.Tags.GetTagsBySearchTerm;
using Nexus.Application.Features.Tags.GetTopTags;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
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
    o.ApplicationAssembly = typeof(CreateImagePostCommandHandler).Assembly;
    o.Durability.Mode = DurabilityMode.MediatorOnly;
    o.Policies.AutoApplyTransactions();
    o.UseFluentValidation();
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

api.MapGet("/tags/top",
    async ([AsParameters] GetTopTagsQuery request, IMessageBus bus) =>
        await bus.InvokeAsync<Result<PagedResult<TagCount>>>(request));

api.MapPostToWolverine<GetTagsBySearchTermQuery, Result<PagedResult<TagCount>>>("/tags/search");
api.MapPostToWolverine<CreateImagePostCommand, Result>("/imageposts");

app.Run();
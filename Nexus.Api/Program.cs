using System.Text.Json.Serialization;
using FluentValidation;
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

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("postgres", configureDataSourceBuilder: o =>
{
    o.ConfigureTracing(c =>
    {
        c.ConfigureCommandFilter(f => !f.CommandText.Contains("HighWaterMark") && !f.CommandText.Contains("wolverine_control_queue"));
    });
});

builder.UseWolverine(o =>
{
    o.Discovery.IncludeAssembly(typeof(CreateImagePostCommandHandler).Assembly);

    o.Policies.AutoApplyTransactions();
    o.Policies.AddMiddleware(typeof(MartenUserMiddleware));
    o.UseFluentValidation();

    o.UseRabbitMqUsingNamedConnection("rabbitmq")
        .AutoProvision();

    o.PublishMessage<ProcessImageCommand>()
        .ToRabbitQueue("image-processing");

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

// Register ClaimsTransformation to enrich claims with user data
builder.Services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSerilogRequestLogging();

// In Development, use custom middleware to convert validation exceptions to ProblemDetails
// This keeps error messages clear while returning proper status codes
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        try
        {
            await next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "Request cannot be processed",
                status = 422,
                errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage })
            });
        }
    });
}
else
{
    app.UseExceptionHandler();
}

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
api.MapCollectionEndpoints();
api.MapAuthEndpoints();


await app.RunJasperFxCommands(args);
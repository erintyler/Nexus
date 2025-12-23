using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Nexus.Application.Common.Services;
using Nexus.Application.Configuration.Models;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Projections;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Domain.Entities;
using Nexus.Infrastructure;
using Serilog;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

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
    
    o.Policies.AutoApplyTransactions();
    
    o.UseRabbitMqUsingNamedConnection("rabbitmq")
        .AutoProvision();

    o.ListenToRabbitQueue("image-processing");

    o.Services.CritterStackDefaults(c =>
    {
        c.Production.GeneratedCodeMode = TypeLoadMode.Static;
        c.Production.ResourceAutoCreate = AutoCreate.None;
    });
});

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddMarten(o =>
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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ImageOptions>(builder.Configuration.GetSection(nameof(ImageOptions)));
builder.Services.AddSingleton<IImageConversionService, ImageConversionService>();
builder.Services.AddSingleton<IThumbnailService, ThumbnailService>();
builder.Services.AddSingleton<IImageService, ImageService>();

var host = builder.Build();
host.Run();
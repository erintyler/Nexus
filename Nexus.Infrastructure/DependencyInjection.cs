using Amazon.S3;
using LocalStack.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Infrastructure.Repositories;
using Nexus.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;

namespace Nexus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalStack(configuration);
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());

        // Register AWS S3 client
        services.AddAwsService<IAmazonS3>(useServiceUrl: true);

        // Register S3 service
        services.AddSingleton<IStorageService, S3StorageService>();

        // Configure FusionCache
        services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(5);
                options.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal;
            });

        // Register repositories with decorator pattern for caching
        services.AddScoped<ITagMigrationRepository, TagMigrationRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(provider =>
        {
            var innerRepository = provider.GetRequiredService<UserRepository>();
            var cache = provider.GetRequiredService<IFusionCache>();
            return new CachedUserRepository(innerRepository, cache);
        });

        // Register auth services
        services.AddHttpClient("Discord", client =>
        {
            client.BaseAddress = new Uri("https://discord.com/api/");
        });

        services.AddScoped<IDiscordApiService, DiscordApiService>();

        return services;
    }
}
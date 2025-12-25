using Amazon.S3;
using LocalStack.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Infrastructure.Repositories;
using Nexus.Infrastructure.Services;

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

        // Register repositories
        services.AddScoped<ITagMigrationRepository, TagMigrationRepository>();

        return services;
    }
}
using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(Amazon.RegionEndpoint.USEast1);

var localstack = builder.AddLocalStack(awsConfig: awsConfig, configureContainer: container =>
{
    container.Lifetime = ContainerLifetime.Persistent;
    container.DebugLevel = 1;
    container.LogLevel = LocalStackLogLevel.Debug;
});

var awsStack = builder.AddAWSCDKStack("aws-stack")
    .WithReference(localstack)
    .WithReference(awsConfig);

var originalImageBucket = awsStack.AddS3Bucket(builder.Configuration["OriginalImageBucketName"] ?? "nexus-original-images");
var processedImageBucket = awsStack.AddS3Bucket(builder.Configuration["ProcessedImageBucketName"] ?? "nexus-processed-images");
var thumbnailBucket = awsStack.AddS3Bucket(builder.Configuration["ThumbnailBucketName"] ?? "nexus-thumbnails");

builder.UseLocalStack(localstack);

var apiService = builder.AddProject<Projects.Nexus_Api>("nexus-api")
    .WithReference(postgres)
    .WithReference(originalImageBucket)
    .WithReference(processedImageBucket)
    .WithReference(thumbnailBucket)
    .WaitFor(postgres);

if (args.Contains("db-patch"))
{
    var fileName = args[2];
    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    
    apiService.WithArgs("db-patch", $"../Nexus.Migrations/Scripts/{timestamp}_{fileName}.sql", "-d", "postgresql://localhost/");
}
else
{
    postgres.WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume();
}

builder.Build().Run();
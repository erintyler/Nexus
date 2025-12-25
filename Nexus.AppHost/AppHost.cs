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

var awsResources = builder.AddAWSCloudFormationTemplate("aws-stack", "cloudformation-stack.yaml")
    .WithReference(awsConfig);

builder.UseLocalStack(localstack);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var apiService = builder.AddProject<Projects.Nexus_Api>("nexus-api")
    .WithReference(postgres)
    .WithReference(awsResources)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.Nexus_ImageProcessor>("nexus-imageprocessor")
    .WithReference(postgres)
    .WithReference(awsResources)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.Nexus_Frontend>("nexus-frontend");

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
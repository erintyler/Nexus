var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");

var apiService = builder.AddProject<Projects.Nexus_Api>("nexus-api")
    .WithReference(postgres)
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
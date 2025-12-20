var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var apiService = builder.AddProject<Projects.Nexus_Api>("nexus-api")
    .WithReference(postgres);

if (args.Contains("db-patch"))
{
    var fileName = args[2];
    
    apiService.WithArgs("db-patch", $"Migrations/{fileName}.sql", "-d", "postgresql://localhost/");
}

builder.Build().Run();
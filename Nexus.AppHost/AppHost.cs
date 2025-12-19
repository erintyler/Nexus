var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

builder.AddProject<Projects.Nexus_Api>("nexus-api")
    .WithReference(postgres);

builder.Build().Run();
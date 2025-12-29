# Nexus.Aspire.IntegrationTests

Integration tests for the Nexus application using .NET Aspire's testing infrastructure. These tests validate the complete application stack with full service orchestration and real dependencies.

## Overview

This test project uses **Aspire.Hosting.Testing** to create integration tests that:

- Spin up the complete application with all dependencies (PostgreSQL, RabbitMQ, LocalStack)
- Test service-to-service communication
- Validate health checks and observability
- Ensure proper startup order and dependency management
- Test end-to-end workflows across multiple services

## Test Categories

### ApiServiceTests

Tests for the Nexus API service focusing on:
- Service startup and readiness
- Health endpoint validation
- Dependency orchestration
- HTTP endpoint accessibility

### ImageProcessorServiceTests

Tests for the ImageProcessor background service:
- Worker service startup
- Dependency wait conditions
- Health check availability
- RabbitMQ message processing readiness

### InfrastructureTests

Tests for infrastructure dependencies:
- PostgreSQL database startup and health
- RabbitMQ message broker configuration
- Connection string availability
- Resource initialization order

### EndToEndTests

Complete workflow tests validating:
- Full application startup with all services
- Service discovery and inter-service communication
- Shared infrastructure access
- Graceful shutdown scenarios
- Real-world usage patterns

## Best Practices

### 1. Resource Lifecycle Management

```csharp
await using var app = await appHost.BuildAsync();
await app.StartAsync();
// Tests run here
// Automatic cleanup via DisposeAsync
```

### 2. Wait for Resource Readiness

```csharp
var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
await resourceNotificationService.WaitForResourceAsync("nexus-api", KnownResourceStates.Running)
    .WaitAsync(TimeSpan.FromSeconds(30));
```

### 3. HTTP Client Configuration

```csharp
appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
{
    clientBuilder.AddStandardResilienceHandler();
});

var httpClient = app.CreateHttpClient("nexus-api");
```

### 4. Connection String Access

```csharp
var connectionString = await app.GetConnectionStringAsync("postgres");
```

## Running the Tests

### Run all Aspire integration tests:
```bash
dotnet test Nexus.Aspire.IntegrationTests
```

### Run a specific test class:
```bash
dotnet test Nexus.Aspire.IntegrationTests --filter "FullyQualifiedName~ApiServiceTests"
```

### Run with detailed output:
```bash
dotnet test Nexus.Aspire.IntegrationTests --logger "console;verbosity=detailed"
```

## Prerequisites

- .NET 10 SDK
- Docker Desktop (running)
- Sufficient system resources (tests will start multiple containers)

## Test Isolation

Each test creates its own isolated instance of the application:
- Fresh containers for each test class
- No shared state between tests
- Automatic cleanup after test completion
- Parallel test execution is safe within constraints

## Timeout Considerations

Default timeouts are set conservatively:
- Infrastructure services: 60 seconds
- Application services: 30-60 seconds
- Health checks: 30 seconds

Adjust timeouts based on your hardware capabilities:

```csharp
await resourceNotificationService.WaitForResourceAsync("postgres", KnownResourceStates.Running)
    .WaitAsync(TimeSpan.FromSeconds(120)); // Increase for slower machines
```

## Troubleshooting

### Tests fail with timeout errors

**Solution**: Increase the timeout values or ensure Docker Desktop has sufficient resources allocated.

### Port conflicts

**Solution**: Aspire automatically assigns random ports. If you still encounter conflicts, ensure no other instances of the application are running.

### Container startup failures

**Solution**: Check Docker Desktop is running and has sufficient disk space. Review the test output for specific container errors.

### Flaky tests

**Solution**: Ensure you're waiting for `KnownResourceStates.Running` before making assertions. Add additional wait time if necessary.

## Configuration

Tests use the AppHost configuration from `Nexus.AppHost/appsettings.json` and can be overridden with environment variables:

```csharp
var appHost = await DistributedApplicationTestingBuilder
    .CreateAsync<Projects.Nexus_AppHost>();

// Configure test-specific settings
appHost.Configuration["ImageOptions:UseLocalStack"] = "true";
```

## Integration with CI/CD

These tests are suitable for CI/CD pipelines with Docker support:

```yaml
# Example GitHub Actions workflow
- name: Run Aspire Integration Tests
  run: dotnet test Nexus.Aspire.IntegrationTests --no-build
  env:
    DOCKER_HOST: unix:///var/run/docker.sock
```

## Additional Resources

- [.NET Aspire Testing Documentation](https://learn.microsoft.com/dotnet/aspire/testing/)
- [Aspire.Hosting.Testing API Reference](https://learn.microsoft.com/dotnet/api/aspire.hosting.testing)
- [xUnit Documentation](https://xunit.net/)
- [Main Nexus README](../README.md)

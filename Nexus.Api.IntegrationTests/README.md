# Nexus API Integration Tests

This project contains comprehensive integration tests for the Nexus API using .NET Aspire tooling.

## Overview

The integration tests use the .NET Aspire hosting infrastructure to spin up the full application stack including:
- Nexus API
- PostgreSQL database
- RabbitMQ message broker
- LocalStack (for AWS services simulation)

## Test Coverage

### Authentication Endpoints (`/api/auth`)
- ✅ POST `/auth/exchange` - Exchange Discord OAuth token for JWT

### Image Endpoints (`/api/images`)
- ✅ POST `/images` - Create new image post
- ✅ GET `/images/search` - Search images by tags with pagination
- ✅ GET `/images/{id}` - Get image by ID
- ✅ PUT `/images/{id}/upload-complete` - Mark image upload as complete
- ✅ POST `/images/{id}/tags` - Add tags to image
- ✅ DELETE `/images/{id}/tags` - Remove tags from image
- ✅ GET `/images/{id}/history` - Get image history with pagination

### Tag Endpoints (`/api/tags`)
- ✅ GET `/tags/search` - Search tags with optional filtering
- ✅ POST `/tags/migrate` - Migrate tags across all image posts
- ✅ GET `/tags/migrations` - Get tag migration history with filtering

### Collection Endpoints (`/api/collections`)
- ✅ POST `/collections` - Create new collection
- ✅ GET `/collections/{id}` - Get collection by ID
- ✅ POST `/collections/{id}/images/{imagePostId}` - Add image to collection
- ✅ DELETE `/collections/{id}/images/{imagePostId}` - Remove image from collection

## Prerequisites

To run these integration tests, you need:

1. **Docker Desktop** - Required for running containers (PostgreSQL, RabbitMQ, LocalStack)
2. **.NET 10 SDK** - Required for building and running the tests
3. **Aspire workload** - Install with: `dotnet workload install aspire`

## Running the Tests

### From Visual Studio
1. Open the solution in Visual Studio 2022 or later
2. Open Test Explorer
3. Run all tests in `Nexus.Api.IntegrationTests`

### From Command Line
```bash
# Run all integration tests
dotnet test Nexus.Api.IntegrationTests

# Run with detailed output
dotnet test Nexus.Api.IntegrationTests --verbosity detailed

# Run specific test
dotnet test Nexus.Api.IntegrationTests --filter "FullyQualifiedName~CreateImage_WithValidData_ReturnsCreatedImage"
```

### From Rider
1. Open the solution in JetBrains Rider
2. Navigate to the test file
3. Click the run icon next to the test class or individual test methods

## Architecture

### AspireAppHostFixture
The `AspireAppHostFixture` class is the core test fixture that:
- Initializes the .NET Aspire application host
- Starts all required services (API, database, message broker, etc.)
- Provides an HTTP client configured to communicate with the API
- Cleans up resources after tests complete

### Test Organization
Tests are organized into four main test classes:
- `AuthEndpointsTests.cs` - Authentication endpoint tests
- `ImageEndpointsTests.cs` - Image management endpoint tests
- `TagEndpointsTests.cs` - Tag management endpoint tests
- `CollectionEndpointsTests.cs` - Collection management endpoint tests

All test classes share the same `AspireAppHostFixture` instance via xUnit's collection fixture feature, ensuring tests run sequentially to avoid resource conflicts.

## Test Patterns

### Arrange-Act-Assert
All tests follow the AAA pattern:
```csharp
[Fact]
public async Task CreateImage_WithValidData_ReturnsCreatedImage()
{
    // Arrange - Set up test data
    var command = new CreateImagePostCommand(...);
    
    // Act - Execute the operation
    var response = await _client.PostAsJsonAsync("/api/images", command);
    
    // Assert - Verify results
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var result = await response.Content.ReadFromJsonAsync<CreateImagePostResponse>();
    Assert.NotNull(result);
}
```

### Test Data Generation
Tests use:
- Unique GUIDs for ensuring test isolation
- Specific tag values with prefixes to avoid collisions
- The actual DTOs and commands from the application

## Troubleshooting

### Docker Issues
If tests fail with Docker-related errors:
- Ensure Docker Desktop is running
- Check that required ports are available (5432 for PostgreSQL, 5672 for RabbitMQ)
- Try `docker system prune` to clean up old containers

### Timeout Issues
If tests timeout during startup:
- The Aspire orchestrator may take time to download and start containers on first run
- Increase the test timeout in your test runner settings
- Check Docker resource limits (CPU, memory)

### Connection Issues
If API calls fail:
- Check that the Aspire app host successfully started all services
- Look for error messages in the test output
- Verify no firewall or network issues blocking local connections

## CI/CD Considerations

When running these tests in CI/CD pipelines:
1. Ensure Docker is available in the build agent
2. Allow sufficient timeout for container startup (2-3 minutes)
3. Consider using cached Docker images to speed up test execution
4. Clean up Docker resources after test runs to avoid disk space issues

## Related Documentation

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [xUnit.net Documentation](https://xunit.net/)
- [ASP.NET Core Testing Documentation](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

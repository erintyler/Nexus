# Nexus API Integration Tests

This project contains comprehensive integration tests for the Nexus API endpoints using .NET Aspire's testing framework.

## Overview

These integration tests use **Aspire.Hosting.Testing** to spin up the full distributed application stack, including:
- PostgreSQL database
- RabbitMQ message broker
- LocalStack (AWS services emulator)
- The Nexus API application

## Prerequisites

To run these tests, you need:
- Docker Desktop or Docker Engine running
- .NET 10 SDK
- At least 4GB of available RAM for containers

## Running the Tests

```bash
# From the Nexus.Api.IntegrationTests directory
dotnet test

# Or from the solution root
dotnet test Nexus.Api.IntegrationTests/Nexus.Api.IntegrationTests.csproj
```

## Test Coverage

The integration tests cover all API endpoints organized by feature:

### Authentication Endpoints (`/api/auth`)
- ✅ POST `/api/auth/exchange` - Exchange Discord OAuth token for JWT

### Image Endpoints (`/api/images`)
- ✅ POST `/api/images` - Create new image
- ✅ GET `/api/images/search` - Search images by tags (with pagination)
- ✅ GET `/api/images/{id}` - Get image by ID
- ✅ PUT `/api/images/{id}/upload-complete` - Mark image upload complete
- ✅ POST `/api/images/{id}/tags` - Add tags to image
- ✅ DELETE `/api/images/{id}/tags` - Remove tags from image
- ✅ GET `/api/images/{id}/history` - Get image history (with pagination)

### Collection Endpoints (`/api/collections`)
- ✅ POST `/api/collections` - Create new collection
- ✅ GET `/api/collections/{id}` - Get collection by ID
- ✅ POST `/api/collections/{id}/images/{imagePostId}` - Add image to collection
- ✅ DELETE `/api/collections/{id}/images/{imagePostId}` - Remove image from collection
- ✅ Verify collection tag aggregation from child images

### Tag Endpoints (`/api/tags`)
- ✅ GET `/api/tags/search` - Search tags (with pagination)
- ✅ POST `/api/tags/migrate` - Migrate tags across posts
- ✅ GET `/api/tags/migrations` - Get tag migration history (with pagination and filtering)

## Test Structure

### Fixtures

**`ApiFixture`** - Manages the lifetime of the distributed application for tests:
- Creates and starts the .NET Aspire app host
- Provides an `HttpClient` configured for the API
- Cleans up resources after tests complete

### Test Classes

Each endpoint group has its own test class:
- `ImageEndpointsTests` - Tests for image management endpoints
- `CollectionEndpointsTests` - Tests for collection management endpoints
- `TagEndpointsTests` - Tests for tag management and migration endpoints
- `AuthEndpointsTests` - Tests for authentication endpoints

## Notes

- Tests use xUnit v3 with the Microsoft Testing Platform
- Each test class uses `IClassFixture<ApiFixture>` to share the distributed application instance
- The Aspire app host automatically provisions all required infrastructure
- Tests validate both successful responses and error conditions
- All tests use realistic data and verify both status codes and response content

## Troubleshooting

### Tests timing out
If tests timeout during initialization:
1. Ensure Docker is running and has sufficient resources allocated
2. Check that no other containers are consuming resources
3. Increase the timeout if necessary (default is 20 seconds)

### Container startup failures
If containers fail to start:
1. Verify Docker Desktop is running
2. Check Docker logs for specific container errors
3. Ensure required ports are not already in use (5432 for Postgres, 5672 for RabbitMQ, etc.)

### Connection errors
If tests fail to connect to the API:
1. Check that the Aspire app host successfully started
2. Verify the API container is healthy
3. Review the test output for connection-specific error messages

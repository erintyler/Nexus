# Nexus

A modern, event-sourced image sharing platform built with .NET 10 and following Domain-Driven Design (DDD) and Clean Architecture principles.

## 🏗️ Architecture

Nexus is structured as a modular monolith following Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation                          │
│                    (Nexus.Api, AppHost)                      │
├─────────────────────────────────────────────────────────────┤
│                        Application                           │
│              (Commands, Queries, Handlers)                   │
├─────────────────────────────────────────────────────────────┤
│                          Domain                              │
│         (Entities, Events, Value Objects, Rules)             │
├─────────────────────────────────────────────────────────────┤
│                      Infrastructure                          │
│              (Repositories, External Services)               │
└─────────────────────────────────────────────────────────────┘
```

### Key Architectural Patterns

- **Event Sourcing**: Domain events are the source of truth using [Marten](https://martendb.io/)
- **CQRS**: Separate command and query responsibilities using [Wolverine](https://wolverinefx.io/)
- **Domain-Driven Design**: Rich domain model with aggregates, value objects, and domain events
- **Clean Architecture**: Dependencies flow inward, domain layer has zero external dependencies
- **Vertical Slice Architecture**: Features organized by business capability

## 📦 Projects

### Core Projects

#### **Nexus.Domain**
Pure domain layer with zero external dependencies. Contains:
- **Entities**: Aggregate roots (`ImagePost`, `TagMigration`)
- **Value Objects**: Immutable domain concepts (`Tag`, `Comment`)
- **Domain Events**: Event sourcing events for all state changes
- **Domain Errors**: Type-safe error handling using Result pattern
- **Business Rules**: Encapsulated validation and invariants

#### **Nexus.Application**
Application business logic and orchestration:
- **Features**: Organized by vertical slices (ImagePosts, Tags, Comments)
- **Commands/Queries**: CQRS command and query handlers using Wolverine
- **Projections**: Read model projections from event streams
- **Validators**: FluentValidation rules for commands/queries
- **DTOs**: Data transfer objects for API responses

#### **Nexus.Infrastructure**
External concerns and third-party integrations:
- **Repositories**: Data access implementations
- **External Services**: AWS S3 integration via LocalStack
- **Image Processing**: SkiaSharp-based image conversion and thumbnails

#### **Nexus.Api**
HTTP API layer using minimal APIs:
- **Endpoints**: HTTP endpoints for images and tags
- **Middleware**: Authentication, error handling, request/response logging
- **API Documentation**: OpenAPI/Scalar integration

### Service Projects

#### **Nexus.AppHost**
.NET Aspire orchestration host:
- Coordinates all services and dependencies
- Manages PostgreSQL, RabbitMQ, LocalStack containers
- Provides observability and telemetry

#### **Nexus.ImageProcessor**
Background worker service:
- Processes image upload events from RabbitMQ queue
- Converts images to WebP format
- Generates multiple thumbnail sizes
- Updates image post status via events

#### **Nexus.ServiceDefaults**
Shared configuration for all services:
- OpenTelemetry configuration
- Health checks
- Common middleware

### Data Projects

#### **Nexus.Migrations**
Database schema management:
- SQL migration scripts
- Version-controlled schema changes
- Integrated with Marten's `db-patch` tooling

### Test Projects

#### **Nexus.Domain.UnitTests**
Domain layer unit tests:
- Entity behavior and business rules
- Value object validation
- Domain event generation

#### **Nexus.Application.UnitTests**
Application layer unit tests:
- Command/query handler logic
- Projection behavior
- Validation rules

#### **Nexus.Architecture.Tests**
Architecture validation tests using NetArchTest:
- Layer dependency rules
- Naming conventions
- DDD pattern enforcement
- See [Architecture Tests README](Nexus.Architecture.Tests/README.md) for details

#### **Nexus.Api.IntegrationTests**
Full-stack integration tests:
- HTTP endpoint testing
- End-to-end workflows

#### **Nexus.ImageProcessor.IntegrationTests**
Image processor service integration tests

#### **Nexus.Migrations.IntegrationTests**
Database migration validation tests

### Utility Projects

#### **Nexus.UnitTests.Utilities**
Shared test utilities:
- AutoFixture extensions
- Test data builders

#### **Nexus.IntegrationTests.Utilities**
Shared integration test infrastructure:
- Test fixtures
- Database seeding helpers

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET Aspire Workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

Install .NET Aspire:
```bash
dotnet workload update
dotnet workload install aspire
```

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Nexus
   ```

2. **Start the application with Aspire**
   ```bash
   dotnet run --project Nexus.AppHost
   ```

   This will start:
   - Nexus.Api (HTTP API)
   - Nexus.ImageProcessor (Background worker)
   - PostgreSQL (Database)
   - RabbitMQ (Message broker)
   - LocalStack (AWS S3 emulator)
   - Aspire Dashboard (Observability)

3. **Access the services**
   
   When running via Aspire AppHost, services are assigned dynamic ports. Access them through the Aspire Dashboard:
   - **Aspire Dashboard**: http://localhost:15017 (check console output for exact URL)
   
   When running the API directly (without Aspire):
   - **API**: http://localhost:5157 or https://localhost:7201
   - **API Documentation (Scalar)**: https://localhost:7201/scalar/v1
   
   > **Note**: .NET Aspire assigns ports dynamically for each service. Use the Aspire Dashboard to find the actual endpoint URLs for each service.

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Nexus.Domain.UnitTests
dotnet test Nexus.Architecture.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🛠️ Technology Stack

### Core Framework
- **.NET 10**: Latest .NET platform
- **C# 13**: Modern C# language features

### Event Sourcing & CQRS
- **[Marten](https://martendb.io/)**: Event store and document database on PostgreSQL
- **[Wolverine](https://wolverinefx.io/)**: Next-generation message handling and CQRS
- **PostgreSQL**: Event store and read model database

### Messaging
- **RabbitMQ**: Asynchronous message broker
- **Wolverine.RabbitMQ**: RabbitMQ integration

### Image Processing
- **SkiaSharp**: Cross-platform image manipulation

### Cloud & Infrastructure
- **AWS S3**: Object storage (via LocalStack in dev)
- **LocalStack**: Local AWS cloud stack emulator
- **Docker**: Containerization

### Observability
- **.NET Aspire**: Cloud-ready orchestration and observability
- **OpenTelemetry**: Distributed tracing and metrics
- **Serilog**: Structured logging

### Testing
- **xUnit**: Test framework with Microsoft.Testing.Platform runner
- **AutoFixture**: Test data generation
- **FluentValidation**: Input validation
- **NetArchTest**: Architecture rule enforcement

### API & Documentation
- **Minimal APIs**: Lightweight HTTP endpoints
- **Scalar**: Modern OpenAPI documentation UI

## 📝 Development Workflow

### Adding a New Feature

1. **Define Domain Events** in `Nexus.Domain/Events/`
2. **Create/Update Entities** in `Nexus.Domain/Entities/`
3. **Write Unit Tests** in `Nexus.Domain.UnitTests/`
4. **Create Command/Query** in `Nexus.Application/Features/`
5. **Add Handler** for the command/query
6. **Create Projection** for read models (if needed)
7. **Add Endpoint** in `Nexus.Api/Endpoints/`
8. **Write Integration Tests** in `Nexus.Api.IntegrationTests/`

### Creating Database Migrations

The solution includes tooling to generate and apply database migrations:

```bash
# Generate a new migration
dotnet run --project Nexus.AppHost -- db-patch <migration-name>

# Example
dotnet run --project Nexus.AppHost -- db-patch add_user_profile
```

This creates a timestamped SQL file in `Nexus.Migrations/Scripts/`.

### Code Style & Conventions

- **Entities**: Must inherit from `BaseEntity` and be sealed
- **Value Objects**: Must inherit from `ValueObject` and be sealed
- **Domain Events**: Must implement `INexusEvent` and end with "DomainEvent"
- **Commands**: End with "Command"
- **Queries**: End with "Query"
- **Validators**: End with "Validator"
- **DTOs**: End with "Dto"

These conventions are enforced by architecture tests. See [Architecture Tests README](Nexus.Architecture.Tests/README.md).

## 🏛️ Domain Model

### Core Aggregates

#### **ImagePost**
The main aggregate representing an image post:
- User-uploaded images with title and tags
- Comments from users
- Upload status lifecycle (Pending → Processing → Completed/Failed)
- Event-sourced: rebuilt from domain events

#### **TagMigration**
Manages tag renaming and consolidation:
- Allows migrating posts from one tag to another
- Maintains tag consistency across the system

### Domain Events

All state changes are captured as domain events:
- `ImagePostCreatedDomainEvent`
- `TagAddedDomainEvent` / `TagRemovedDomainEvent`
- `CommentCreatedDomainEvent` / `CommentUpdatedDomainEvent` / `CommentDeletedDomainEvent`
- `StatusChangedDomainEvent`
- `TagMigratedDomainEvent`

### Value Objects

- **Tag**: Immutable tag with type and value
- **Comment**: Immutable comment with content and metadata
- **TagData**: Data structure for tag type and value

## 🔧 Configuration

### Application Settings

Configuration is managed through `appsettings.json` and environment-specific overrides:

```json
{
  "ImageOptions": {
    "OriginalImageBucketName": "nexus-original-images",
    "ProcessedImageBucketName": "nexus-processed-images",
    "ThumbnailBucketName": "nexus-thumbnails",
    "ProcessedImagePublicDomain": "https://nexus-processed-images.s3.localhost.localstack.cloud:4566",
    "ThumbnailPublicDomain": "https://nexus-thumbnails.s3.localhost.localstack.cloud:4566"
  },
  "LocalStack": {
    "UseLocalStack": true
  }
}
```

### Connection Strings

In development, .NET Aspire manages connection strings automatically. For production:

```json
{
  "ConnectionStrings": {
    "postgres": "Host=localhost;Database=nexus;Username=postgres;Password=***",
    "rabbitmq": "amqp://guest:guest@localhost:5672"
  }
}
```

## 🤝 Contributing

1. Follow the existing architecture and coding patterns
2. Write tests for new features
3. Run architecture tests to ensure compliance: `dotnet test Nexus.Architecture.Tests`
4. Ensure all tests pass before submitting changes
5. Follow event sourcing principles - state changes through events only

## 📄 License

[Your License Here]

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Marten Event Sourcing](https://martendb.io/events/)
- [Wolverine Documentation](https://wolverinefx.io/)
- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/)
- [Architecture Tests README](Nexus.Architecture.Tests/README.md)

---

Built with ❤️ using .NET 10 and modern software architecture principles.


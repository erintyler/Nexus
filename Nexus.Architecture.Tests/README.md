# Architecture Tests Documentation

This project implements comprehensive architecture tests following Domain-Driven Design (DDD) best practices using **NetArchTest.Rules** and **xUnit** assertions.

## Test Categories

### 1. Domain Layer Tests (`DomainLayerTests.cs`)

Tests that ensure the Domain layer follows DDD principles and maintains proper isolation:

- **Independence Tests**: Domain should not depend on Application, API, or Infrastructure layers
- **External Library Independence**: Domain should only use basic .NET types, no external dependencies
- **Entity Rules**: All entities must inherit from `Entity` base class and be sealed
- **Value Object Rules**: All value objects must inherit from `ValueObject` base class and be sealed
- **Domain Event Rules**: 
  - Must implement `INexusEvent` interface
  - Must be immutable (records or readonly properties)
  - Must have names ending with "DomainEvent"

### 2. Application Layer Tests (`ApplicationLayerTests.cs`)

Tests that validate the Application layer follows clean architecture principles:

- **Dependency Rules**: Application should only depend on Domain, never on API/Infrastructure
- **No Infrastructure Dependencies**: Should not reference EF Core, Npgsql, etc.
- **Command/Query Naming**: Commands end with "Command", Queries end with "Query"
- **Validator Naming**: Validators end with "Validator"
- **Service Patterns**: Services must have corresponding interfaces starting with "I"
- **DTO Naming**: DTOs in Models namespace must end with "Dto"
- **Handler Encapsulation**: Handlers should not be public (internal/private only)

### 3. API Layer Tests (`ApiLayerTests.cs`)

Tests that ensure the API layer serves as a thin presentation layer:

- **Separation of Concerns**: API should not directly reference domain entities (use DTOs)
- **Endpoint Naming**: Endpoint classes must end with "Endpoints"
- **Endpoint Design**: All endpoint classes should be sealed
- **Middleware Organization**: Middleware must reside in Middleware namespace
- **Exception Handler Naming**: Exception handlers end with "ExceptionHandler"
- **No Controllers**: Project uses minimal APIs, not MVC controllers
- **Extension Organization**: Extension methods must be in Extensions namespace
- **No Business Logic**: API endpoints should not contain business logic classes

### 4. Layer Dependency Tests (`LayerDependencyTests.cs`)

Tests that enforce proper dependency direction following Clean Architecture:

- **Domain Independence**: Domain layer completely independent
- **Application Isolation**: Application only depends on Domain
- **No Circular Dependencies**: Enforces one-way dependency flow
- **Internal Layer Rules**:
  - Primitives don't depend on Entities
  - Value Objects don't depend on Entities
  - Events don't depend on Entities
  - Common doesn't depend on Features
  - Features don't depend on each other
  - Endpoints don't depend on other Endpoints

### 5. Naming Convention Tests (`NamingConventionTests.cs`)

Tests that ensure consistent naming conventions across the codebase:

- **Interface Naming**: All interfaces start with "I"
- **Abstract Class Naming**: Abstract classes start with "Base"/"Abstract" or end with "Base"
- **No Hungarian Notation**: Classes should not use "C" prefix
- **Public Class Organization**: Public classes not in Internal namespaces
- **Enum Naming**: Enums have singular names (not plural)
- **Extension Class Naming**: Extension classes end with "Extensions" (excludes Endpoints classes)
- **Constants Organization**: Classes with many constants should have descriptive names
- **Async Method Naming**: Async methods must have "Async" suffix

## Test Philosophy

These tests are **guardrails**, not gatekeepers. They:

1. **Enforce architectural boundaries** to prevent accidental coupling
2. **Detect code smells** early in development
3. **Document architectural decisions** through executable specifications
4. **Enable refactoring confidence** by catching breaking changes
5. **Guide new developers** toward proper patterns

## Running the Tests

```bash
# Run all architecture tests
dotnet test Nexus.Architecture.Tests/Nexus.Architecture.Tests.csproj

# Run with detailed output
dotnet test Nexus.Architecture.Tests/Nexus.Architecture.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~DomainLayerTests"
```

## Handling Test Failures

When a test fails, it indicates an architectural violation. Consider:

1. **Is this a legitimate issue?** Most failures indicate actual problems
2. **Is the rule too strict?** Some rules may need adjustment for your specific needs
3. **Should the rule be modified?** Tests can be updated if the team agrees on a pattern change
4. **Add exceptions if needed** Use skip attributes or modify predicates for valid exceptions

## Benefits of Architecture Tests

✅ **Prevent Architecture Erosion**: Catches violations before code review
✅ **Self-Documenting**: Tests serve as living documentation
✅ **CI/CD Integration**: Automated enforcement in build pipeline
✅ **Team Alignment**: Codifies team agreements about architecture
✅ **Refactoring Safety**: Ensures architectural integrity during changes

## Common Violations Found

The initial test run identified several areas for improvement:

- Domain entities not inheriting from base Entity class (likely using Marten base)
- Domain events not immutable
- Entities and Value Objects not sealed
- Handlers being public instead of internal
- ReadModel classes should be renamed to DTOs
- Some extension classes need proper naming

These findings demonstrate the tests are working correctly and identifying real architectural issues.

## Integration with CI/CD

Add to your pipeline:

```yaml
- name: Run Architecture Tests
  run: dotnet test Nexus.Architecture.Tests/Nexus.Architecture.Tests.csproj --no-build --verbosity normal
```

## Customization

Feel free to adjust tests based on your team's conventions. The tests are organized by concern and use clear assertion messages to guide developers when violations occur.


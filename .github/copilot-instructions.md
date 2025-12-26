# GitHub Copilot Instructions for Nexus Repository

## Code Formatting

**ALWAYS run `dotnet format` before committing any C# code changes.**

After making any changes to `.cs` files:
1. Run `dotnet format` in the repository root to ensure consistent code formatting
2. Verify formatting compliance with `dotnet format --verify-no-changes`
3. Only commit changes after formatting checks pass

## Code Style Guidelines

- Follow Clean Architecture principles
- Use the Result pattern for error handling instead of throwing exceptions
- Use AutoFixture for test data generation in unit tests
- Follow event sourcing patterns for domain entities
- Use CQRS separation for commands and queries

## Testing

- Write unit tests for all new functionality
- Use AutoFixture's `_fixture.Create<T>()` for generating test data
- Follow the existing test patterns in the codebase
- Ensure all tests pass before committing

## Architecture

- Domain layer must have no external dependencies
- Application layer must not reference Infrastructure
- Use Marten for event sourcing and document storage
- Use Wolverine for message handling and CQRS

# Authentication Dependencies Analysis

## Detailed Dependency Mapping

This document provides technical details about authentication dependencies in the Nexus codebase to support the microservice evaluation.

## Authentication Components and Their Dependencies

### 1. AuthEndpoints (Presentation Layer)
**Location**: `Nexus.Api/Endpoints/AuthEndpoints.cs`

**Dependencies**:
- `Wolverine.IMessageBus` - CQRS message bus
- `ExchangeTokenCommand` - Application layer command
- `ExchangeTokenResponse` - Application layer DTO
- `Result<T>` - Domain error handling pattern
- ASP.NET Core minimal APIs

**Dependency Direction**: Presentation → Application → Domain

### 2. ExchangeTokenCommandHandler (Application Layer)
**Location**: `Nexus.Application/Features/Auth/ExchangeToken/ExchangeTokenCommandHandler.cs`

**Dependencies**:
- `IDiscordApiService` - Infrastructure service
- `IJwtTokenService` - API service (cross-layer concern)
- `IUserRepository` - Infrastructure repository
- `ILogger<T>` - Framework logging
- Domain errors and results

**Key Operations**:
1. Validate Discord token via external API call
2. Get or create user in database (event sourcing)
3. Generate JWT token
4. Return response DTO

**Dependency Issue**: Handler depends on `IJwtTokenService` from API layer, violating Clean Architecture slightly. This is a pragmatic trade-off for JWT generation.

### 3. User Entity (Domain Layer)
**Location**: `Nexus.Domain/Entities/User.cs`

**Properties**:
- `Id: Guid` - Primary key
- `DiscordId: string` - External identity
- `DiscordUsername: string` - Display name
- Event sourcing metadata (CreatedAt, LastModified, LastModifiedBy)

**Domain Events**:
- `UserCreatedDomainEvent` - User registration event

**No External Dependencies** - Pure domain entity ✅

### 4. UserRepository (Infrastructure Layer)
**Location**: `Nexus.Infrastructure/Repositories/UserRepository.cs`

**Dependencies**:
- `Marten.IDocumentSession` - Event store
- `User` entity (Domain)
- `Result<T>` (Domain)

**Operations**:
```csharp
Task<User?> GetByDiscordIdAsync(string discordId, CancellationToken ct)
Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
Task<Result<Guid>> CreateAsync(string discordId, string discordUsername, CancellationToken ct)
```

**Database Schema**:
- Event stream: `User` aggregate
- Document store: `User` projections
- Indexed on: `DiscordId` for fast lookups

### 5. JwtTokenService (API Layer)
**Location**: `Nexus.Api/Services/JwtTokenService.cs`

**Dependencies**:
- `Microsoft.IdentityModel.Tokens`
- `System.IdentityModel.Tokens.Jwt`
- `JwtSettings` configuration

**Generated Claims**:
- `ClaimTypes.NameIdentifier` → Internal User ID (Guid)

**Token Configuration**:
- Expiry: 1 hour
- Algorithm: HMAC-SHA256
- Issuer: Nexus.Api
- Audience: Nexus.Frontend

### 6. DiscordApiService (Infrastructure Layer)
**Location**: `Nexus.Infrastructure/Services/DiscordApiService.cs`

**External Dependency**: Discord API (`https://discord.com/api`)

**Operation**: Validates Discord OAuth token by calling `/users/@me` endpoint

**Failure Modes**:
- Network failures
- Invalid tokens
- Discord API downtime
- Rate limiting

## Cross-Cutting Usage of Authentication

### UserContextService Usage

The `IUserContextService` is injected into multiple locations for audit trails:

1. **MartenUserMiddleware** (`Nexus.Api/Middleware/Wolverine/`)
   - Sets user context for all Marten operations
   - Ensures all events track the user who created them

2. **Application Handlers** (Various locations)
   - Used to get current user ID for command validation
   - Used to enforce authorization rules

**Usage Count**: ~18 references across Application and API layers

### User Entity Usage

The User entity is referenced throughout the system:

1. **Event Metadata** - All domain events include user ID
2. **ImagePost** - Tracks creator and modifier
3. **Comment** - Tracks author
4. **Collection** - Tracks owner
5. **TagMigration** - Tracks who performed migration

**Impact**: User is fundamental to the event sourcing model

## Database Dependencies

### Marten Event Store Schema

```sql
-- Event streams (created by Marten)
CREATE TABLE mt_events (
    seq_id BIGSERIAL PRIMARY KEY,
    stream_id UUID NOT NULL,
    version INT NOT NULL,
    data JSONB NOT NULL,
    type VARCHAR(255) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    -- User tracking metadata
    user_name VARCHAR(255),
    ...
);

-- User document projections
CREATE TABLE mt_doc_user (
    id UUID PRIMARY KEY,
    data JSONB NOT NULL,
    -- Indexed fields
    discord_id VARCHAR(255),
    mt_last_modified TIMESTAMP,
    ...
);

CREATE INDEX idx_user_discord_id ON mt_doc_user(discord_id);
```

**Event Sourcing Impact**: User events are permanently stored in event stream. Extracting authentication would require:
- Replicating user data
- Handling eventual consistency
- Managing distributed transactions

## Network Communication Patterns

### Current (Monolithic)

```
Request Flow (Token Exchange):
1. Frontend → API: POST /api/auth/exchange (1 HTTP call)
2. API → Discord API: GET /users/@me (1 HTTP call)
3. API → Database: Query user (1 DB query)
4. API → Database: Create user if needed (1 DB write)
5. API → Frontend: JWT response

Total Latency: ~100-200ms (mostly Discord API)
Failure Points: 2 (Discord API, Database)
```

### If Authentication Were a Microservice

```
Request Flow (Token Exchange):
1. Frontend → Auth Service: POST /auth/exchange (1 HTTP call)
2. Auth Service → Discord API: GET /users/@me (1 HTTP call)
3. Auth Service → User Service: GET /users/by-discord/{id} (1 HTTP call)
4. User Service → Database: Query user (1 DB query)
5. User Service → Auth Service: User response
6. Auth Service → User Service: POST /users (if new) (1 HTTP call)
7. User Service → Database: Create user (1 DB write)
8. User Service → Auth Service: User created response
9. Auth Service → Frontend: JWT response

Total Latency: ~300-500ms+ (additional service hops)
Failure Points: 4 (Discord API, Auth Service, User Service, Database)
```

**Impact**: 2-3x latency, 2x failure points

## Configuration Dependencies

### Current Configuration

**API Configuration** (`appsettings.json`):
```json
{
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "Nexus.Api",
    "Audience": "Nexus.Frontend"
  }
}
```

**Frontend Configuration**:
```json
{
  "Discord": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "ApiBaseUrl": "https://localhost:7001"
}
```

### If Microservice

Would need additional configuration:
- Service discovery endpoints
- Circuit breaker settings
- Retry policies
- Timeout configurations
- Health check endpoints
- Distributed tracing correlation

## Testing Dependencies

### Current Test Structure

1. **Unit Tests**:
   - `ExchangeTokenCommandHandlerTests` - Tests handler logic with mocked dependencies
   - `ExchangeTokenCommandValidatorTests` - Tests validation rules
   - No network calls, fast execution

2. **Integration Tests**:
   - `AuthEndpointsTests` - Tests full HTTP endpoint with Alba
   - Includes database and Wolverine
   - ~50ms per test

### If Microservice

Would need:
- Contract testing between services
- End-to-end integration tests across services
- Service virtualization for testing
- More complex test infrastructure

## Code Size Breakdown

```
Authentication Code Distribution:

Nexus.Api/
├── Endpoints/AuthEndpoints.cs                    48 lines
├── Services/JwtTokenService.cs                   49 lines
├── Services/UserContextService.cs                20 lines
└── Extensions/TestAuthHandler.cs                 29 lines

Nexus.Application/
└── Features/Auth/ExchangeToken/
    ├── ExchangeTokenCommand.cs                    5 lines
    ├── ExchangeTokenCommandHandler.cs            63 lines
    ├── ExchangeTokenCommandValidator.cs          14 lines
    └── ExchangeTokenResponse.cs                   8 lines

Nexus.Domain/
├── Entities/User.cs                              56 lines
├── Events/Users/UserCreatedDomainEvent.cs        ~10 lines
└── Errors/AuthErrors.cs                          17 lines

Nexus.Infrastructure/
├── Repositories/UserRepository.cs                43 lines
└── Services/DiscordApiService.cs                 38 lines

Total: ~400 lines of code
```

## External Dependencies

### NuGet Packages (Authentication-Related)

```xml
<!-- API Project -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="*" />

<!-- Infrastructure Project -->
<PackageReference Include="HttpClient" (built-in .NET) />
```

**Minimal Dependencies**: Only JWT and HTTP client, both standard .NET libraries

## Performance Characteristics

### Current Performance

- **Token Exchange**: ~100-200ms (Discord API is bottleneck)
- **JWT Validation**: <1ms (in-memory validation)
- **User Lookup**: ~5-10ms (indexed query)
- **User Creation**: ~20-30ms (event sourcing)

### Scaling Limits

- **Requests/Second**: Limited by Discord API rate limits (not internal capacity)
- **Database**: PostgreSQL can handle millions of users
- **Memory**: JWT validation is stateless, infinitely scalable
- **CPU**: Minimal CPU usage for auth operations

**Conclusion**: No performance bottlenecks in current implementation

## Risk Analysis: Extracting Authentication

### High Risk Issues

1. **Data Consistency**
   - User data needs to be synchronized across services
   - Event sourcing integrity would be compromised
   - Risk of split-brain scenarios

2. **Increased Latency**
   - Every request requires network call to auth service OR
   - Complex caching layer with invalidation logic
   - User experience degradation

3. **Operational Complexity**
   - More services to monitor, deploy, scale
   - Distributed tracing becomes mandatory
   - Debugging becomes significantly harder

4. **Single Point of Failure**
   - If auth service is down, entire application is inaccessible
   - Current: Auth failure only affects new logins, existing sessions work
   - Microservice: Auth failure affects token validation

### Medium Risk Issues

1. **Development Velocity**
   - Changes require coordination across services
   - Integration testing becomes more complex
   - Local development requires running multiple services

2. **Cost**
   - Additional infrastructure resources
   - More complex CI/CD pipelines
   - Higher cloud costs

### Low Risk Issues

1. **Technology Lock-in**
   - Current: Using standard .NET authentication
   - Microservice: Still using .NET (no benefit)

## Recommendations

Based on this detailed analysis:

1. **Keep authentication integrated** - Dependencies are too tightly coupled
2. **Focus on modularity** - Current feature folder structure is good
3. **Improve observability** - Add metrics and tracing to existing code
4. **Consider external IdP** - If auth complexity grows, use Auth0/Cognito
5. **Document boundaries** - Maintain clean architecture separation

## Alternative: Module Extraction (Future)

If the system grows significantly (100K+ users, 10+ developers), consider:

**Module Extraction over Microservice**:
- Keep in same process but separate assembly
- Shared database with clear ownership
- In-process communication (no network overhead)
- Easier to refactor to microservice if truly needed

```
Nexus.AuthModule (separate assembly)
├── Interfaces (public API)
├── Implementation (internal)
└── Database (shared, clear schema ownership)

Benefits:
- Logical separation
- No network overhead
- Maintains event sourcing integrity
- Easy to test
- Can be extracted later if needed
```

---

**Key Takeaway**: The authentication implementation is appropriately sized for the current system. Microservice extraction would add significant complexity with no measurable benefit.

# Authentication Architecture Diagrams

## Current Architecture (Integrated Monolith)

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           Frontend (BFF)                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  Discord OAuth → Token Exchange → JWT Cookie                        │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │ HTTPS + JWT
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                              Nexus.Api                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │ /api/auth/exchange  │  JWT Validation  │  User Context               │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │ In-Process Call
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                         Nexus.Application                                 │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │ ExchangeTokenCommand → Handler → Validation                          │ │
│  │ Uses: IDiscordApiService, IJwtTokenService, IUserRepository         │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │ In-Process Call
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                           Nexus.Domain                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │ User Entity (Event Sourced) │ AuthErrors │ UserCreatedEvent         │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │ In-Process Call
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                       Nexus.Infrastructure                                │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │ UserRepository → Marten (Event Store)                                │ │
│  │ DiscordApiService → Discord API (External)                           │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                    ┌────────────┴──────────────┐
                    ▼                           ▼
          ┌──────────────────┐        ┌──────────────────┐
          │   PostgreSQL     │        │   Discord API    │
          │  (Event Store)   │        │   (External)     │
          └──────────────────┘        └──────────────────┘
```

**Characteristics:**
- ✅ Direct method calls (microseconds)
- ✅ Single deployment unit
- ✅ Shared database with event sourcing
- ✅ Simple debugging and testing
- ✅ Consistent transaction boundaries

---

## Hypothetical Microservice Architecture (NOT Recommended)

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           Frontend (BFF)                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  Discord OAuth → Token Exchange → JWT Cookie                        │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │ HTTPS
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                      Authentication Microservice                          │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │ /auth/exchange  │  JWT Generation  │  Token Validation              │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
└─────────┬──────────────────────────────────┬─────────────────────────────┘
          │                                  │
          │ HTTP Call                        │ HTTP Call
          │ (Latency!)                       │ (Latency!)
          ▼                                  ▼
┌─────────────────────┐            ┌─────────────────────────────────────┐
│   User Service      │            │     Main API Service                │
│  ┌────────────────┐ │            │  ┌───────────────────────────────┐ │
│  │ User CRUD      │ │            │  │ Business Logic                │ │
│  │ User Lookup    │ │◄───HTTP────┤  │ Needs user context for       │ │
│  │ User Creation  │ │            │  │ every operation!              │ │
│  └────────────────┘ │            │  └───────────────────────────────┘ │
└──────────┬──────────┘            └─────────────────────────────────────┘
           │                                         ▲
           │                                         │
           ▼                                         │ Needs user on
    ┌──────────────┐                                │ every request
    │ PostgreSQL   │                                │ (N+1 queries)
    │ (User DB)    │                                │
    └──────────────┘                                │
           ▲                                         │
           │                                         │
           │ Sync?                                   │
           │ Eventually consistent?                  │
           │                                         │
    ┌──────────────┐                                │
    │ PostgreSQL   │────────────────────────────────┘
    │ (Events DB)  │    Distributed transactions!
    └──────────────┘    Data duplication!
```

**Problems:**
- ❌ Network latency on every auth operation
- ❌ Multiple HTTP calls (200-500ms vs <1ms)
- ❌ Distributed transactions complexity
- ❌ Data synchronization challenges
- ❌ More failure points
- ❌ Complex deployment
- ❌ Harder debugging
- ❌ No clear benefits

---

## Authentication Flow Comparison

### Current: Integrated Monolith

```
Token Exchange Flow:
┌─────────┐    1. POST /api/auth/exchange     ┌─────────────┐
│ Frontend│───────────────────────────────────▶│   API       │
└─────────┘                                    │             │
                                               │  2. Validate│
                                               │     token   │
                                               │     (in     │
                                               │   process)  │
                                               │             │
                                               │  3. Get/    │
                                               │     Create  │
                                               │     user    │
                                               │   (in DB)   │
                                               │             │
                                               │  4. Gen JWT │
     ┌─────────────JWT─────────────────────────┤   (in       │
     │                                         │   process)  │
     ▼                                         │             │
┌─────────┐                                    └─────────────┘
│ Frontend│
└─────────┘
                        Total: ~100-200ms
                   (Discord API is bottleneck)
```

### Microservice: If We Did It (NOT Recommended)

```
Token Exchange Flow:
┌─────────┐    1. POST /auth/exchange          ┌──────────────┐
│ Frontend│───────────────────────────────────▶│Auth Service  │
└─────────┘                                    │              │
                                               │  2. HTTP call│
                                               │     to User  │
                                               │     Service  │
                                               │              │
                                        ┌──────┴──────┐       │
                                        │User Service │       │
                                        │             │       │
                                        │ 3. Get/     │       │
                                        │    Create   │       │
                                        │    user     │       │
                                        │    in DB    │       │
                                        │             │       │
                                        │ 4. Return   │       │
                                        │    to Auth  │       │
                                        └──────┬──────┘       │
                                               │              │
                                               │  5. Gen JWT  │
     ┌─────────────JWT──────────────────────────┤             │
     │                                          │             │
     ▼                                          └─────────────┘
┌─────────┐
│ Frontend│
└─────────┘
                        Total: ~300-500ms
              (2x network hops + original Discord)
```

---

## User Entity Integration

### Current: Centralized Event Sourcing

```
                     ┌──────────────────────┐
                     │   User Aggregate     │
                     │   (Event Sourced)    │
                     └──────────┬───────────┘
                                │
                ┌───────────────┼───────────────┐
                │               │               │
                ▼               ▼               ▼
         ┌──────────┐    ┌──────────┐    ┌──────────┐
         │ImagePost │    │ Comment  │    │Collection│
         │ Events   │    │  Events  │    │  Events  │
         └──────────┘    └──────────┘    └──────────┘
                │               │               │
                └───────────────┼───────────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │   Single Event Store  │
                    │   (Full Audit Trail)  │
                    └───────────────────────┘
```

**Benefits:**
- ✅ Single source of truth
- ✅ Complete audit trail
- ✅ Easy to reconstruct system state
- ✅ ACID transactions

### Microservice: Distributed Data (NOT Recommended)

```
┌────────────────────┐              ┌────────────────────┐
│  Auth Service DB   │              │   Main App DB      │
│                    │              │                    │
│  ┌──────────────┐  │              │  ┌──────────────┐  │
│  │ User Table   │  │◄────Sync?────┤  │ User Copy    │  │
│  └──────────────┘  │              │  └──────────────┘  │
│                    │              │                    │
│  User events here  │              │  Other events here │
└────────────────────┘              └────────────────────┘
```

**Problems:**
- ❌ Data duplication
- ❌ Synchronization complexity
- ❌ Split audit trail
- ❌ Eventual consistency
- ❌ Distributed transactions

---

## Complexity Comparison

### Current Implementation

```
Components:      6
HTTP Endpoints:  1
Services:        1 (Nexus.Api)
Databases:       1 (PostgreSQL)
Network Calls:   1 (Discord API only)
Lines of Code:   ~400
Failure Points:  2 (Database, Discord)
Deployment:      Simple
Testing:         Simple
Debugging:       Easy
```

### Microservice Implementation

```
Components:      10+
HTTP Endpoints:  3+
Services:        3+ (Auth, User, Main API)
Databases:       2+ (or shared with complexity)
Network Calls:   4+ (internal + Discord)
Lines of Code:   ~800+ (infrastructure overhead)
Failure Points:  5+ (Auth service, User service, DBs, Discord, Network)
Deployment:      Complex (orchestration, service discovery)
Testing:         Complex (contract tests, integration)
Debugging:       Hard (distributed tracing required)
```

---

## Decision Matrix

| Factor | Current | Microservice | Winner |
|--------|---------|--------------|--------|
| Latency | <200ms | 300-500ms | ✅ Current |
| Complexity | Low | High | ✅ Current |
| Operational Burden | Low | High | ✅ Current |
| Code Size | ~400 LOC | ~800+ LOC | ✅ Current |
| Failure Points | 2 | 5+ | ✅ Current |
| Testing | Simple | Complex | ✅ Current |
| Debugging | Easy | Hard | ✅ Current |
| Cost | Low | High | ✅ Current |
| Scalability | Sufficient | Better | ⚖️ Tie (not needed) |
| Team Autonomy | N/A | Better | ⚖️ Tie (not needed) |

**Score: 8-0 in favor of keeping integrated**

---

## When to Revisit

```
Current State:
┌─────────────────────────────────────────────────────────────┐
│  Users: Unknown (Early Stage)                                │
│  Requests/sec: Low                                           │
│  Development Teams: 1                                        │
│  Auth Complexity: Simple (Discord OAuth + JWT)              │
│                                                              │
│  Recommendation: Keep Integrated ✅                          │
└─────────────────────────────────────────────────────────────┘

Future State (Revisit Decision):
┌─────────────────────────────────────────────────────────────┐
│  Users: 100,000+                                             │
│  Requests/sec: 10,000+                                       │
│  Development Teams: 5+                                       │
│  Auth Complexity: Multi-provider, MFA, RBAC, etc.           │
│                                                              │
│  Consider: Module Extraction or External IdP ⚠️             │
└─────────────────────────────────────────────────────────────┘
```

---

## Recommended Architecture Evolution

```
Phase 1 (Current): Integrated Monolith
┌─────────────────────────────────┐
│   Single Deployment Unit         │
│  ┌────────────────────────────┐ │
│  │ API + Auth + Domain        │ │
│  └────────────────────────────┘ │
└─────────────────────────────────┘
✅ Simple, Fast, Maintainable

Phase 2 (Growth): Enhanced Monolith
┌─────────────────────────────────┐
│   Single Deployment Unit         │
│  ┌────────────────────────────┐ │
│  │ API                        │ │
│  ├────────────────────────────┤ │
│  │ Auth Module (Separate Asm) │ │
│  ├────────────────────────────┤ │
│  │ Domain                     │ │
│  └────────────────────────────┘ │
└─────────────────────────────────┘
✅ Logical Separation, No Network Overhead

Phase 3 (Scale): External IdP (If Needed)
┌──────────────┐    ┌──────────────┐
│   Auth0/     │◄───│  Nexus API   │
│   Cognito    │    │              │
└──────────────┘    └──────────────┘
✅ Professional Security, No Operational Burden

Phase 4 (Massive Scale): Microservice (Maybe)
┌────────────┐  ┌────────────┐  ┌────────────┐
│ Auth       │  │ User       │  │ Main API   │
│ Service    │  │ Service    │  │            │
└────────────┘  └────────────┘  └────────────┘
⚠️ Only if hitting real scale limits (unlikely)
```

---

**Conclusion**: The current integrated architecture is optimal for the system's maturity level. Resist premature microservice extraction.

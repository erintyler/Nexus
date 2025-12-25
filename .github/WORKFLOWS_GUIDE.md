# Quick Reference: GitHub Actions Workflows

## 📊 Workflows Overview

| Workflow | File | Triggers | Duration | Purpose |
|----------|------|----------|----------|---------|
| **PR Pipeline** | `pr.yml` | Pull Requests | ~6-10 min | Validate code quality, tests, builds |
| **CodeQL Security** | `codeql.yml` | Push, PR, Manual | ~7-11 min | Security vulnerability scanning |

## 🎯 PR Pipeline (`pr.yml`)

### Quick Stats
- **Jobs**: 6
- **Test Projects**: 5 (Domain, Application, Frontend, API Integration, Migrations Integration)
- **Testing Platform**: xUnit v3 with Microsoft Testing Platform
- **Dependencies**: Testcontainers for integration tests

### Job Flow
```
code-quality (2-3 min) ──┐
                          │
build (1-2 min) ──────────┼──► architecture-tests (2-3 min) ──┐
                          │                                     │
                          ├──► unit-tests (3-5 min) ───────────┤
                          │    (Domain + App + Frontend)       │
                          │                                     │
                          └──► integration-tests (3-6 min) ────┤
                               (API + Migrations w/ Testcontainers)
                                                                │
                               pr-check-summary (1 min) ◄──────┘
```

### Commands to Run Locally

```bash
# Format check
dotnet format Nexus.slnx --verify-no-changes

# Build
dotnet build Nexus.slnx --configuration Release

# Run all tests
dotnet test Nexus.slnx --configuration Release

# Run specific test project
dotnet test Nexus.Domain.UnitTests/Nexus.Domain.UnitTests.csproj

# Run with verbose output
dotnet test Nexus.slnx --configuration Release -v detailed
```

## 🔒 CodeQL Security (`codeql.yml`)

### Quick Stats
- **Languages**: C#, JavaScript
- **Queries**: security-extended, security-and-quality

### When It Runs
- ✅ Every push to `main` or `develop`
- ✅ Every pull request
- ✅ Manually via Actions tab

### Viewing Results
GitHub → **Security** → **Code scanning alerts**

## 📋 Test Projects Matrix

| Project | Type | Dependencies | Duration |
|---------|------|--------------|----------|
| `Nexus.Architecture.Tests` | Architecture | None | ~2-3 min |
| `Nexus.Domain.UnitTests` | Unit | None | ~1 min |
| `Nexus.Application.UnitTests` | Unit | None | ~1 min |
| `Nexus.Frontend.UnitTests` | Unit | None | ~1 min |
| `Nexus.Api.IntegrationTests` | Integration | Testcontainers | ~2-3 min |
| `Nexus.Migrations.IntegrationTests` | Integration | Testcontainers | ~1-2 min |

## 🎨 Status Badges

Add these to your README.md:

```markdown
[![PR Pipeline](https://github.com/YOUR_USERNAME/Nexus/actions/workflows/pr.yml/badge.svg)](https://github.com/YOUR_USERNAME/Nexus/actions/workflows/pr.yml)
[![CodeQL](https://github.com/YOUR_USERNAME/Nexus/actions/workflows/codeql.yml/badge.svg)](https://github.com/YOUR_USERNAME/Nexus/actions/workflows/codeql.yml)
```

## 🚨 Troubleshooting

### PR Pipeline Failures

**Code Quality Job**
```bash
# Fix locally
dotnet format Nexus.slnx
git add .
git commit -m "chore: format code"
```

**Build Job**
```bash
# Check locally
dotnet restore Nexus.slnx
dotnet build Nexus.slnx --configuration Release
```

**Test Jobs**
```bash
# Run tests with detailed output
dotnet test -v detailed

# Run specific failing test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

**Integration Tests (using Testcontainers)**
```bash
# Testcontainers handles services automatically
# Just run the tests
dotnet test Nexus.Api.IntegrationTests/Nexus.Api.IntegrationTests.csproj
dotnet test Nexus.Migrations.IntegrationTests/Nexus.Migrations.IntegrationTests.csproj

# Ensure Docker is running
docker info
```

### CodeQL Failures

**False Positives**
Add suppressions in `.github/codeql/codeql-config.yml`

**Build Failures**
Ensure all dependencies restore correctly:
```bash
dotnet restore Nexus.slnx
dotnet build Nexus.slnx --no-restore
```

## ⚡ Performance Tips

1. **NuGet Package Caching**
   - Already configured using `setup-dotnet` built-in caching
   - Uses `packages.lock.json` for cache keys
   
2. **Parallel Test Execution**
   - Tests already run in parallel via xUnit v3

3. **Testcontainers**
   - Automatically manages Docker containers for integration tests
   - Cleanup handled automatically

4. **Skip Redundant Restores**
   - Use `--no-restore` flag (already implemented)

## 🔐 Required Permissions

### PR Pipeline
- `contents: read` (default)
- `actions: write` (for artifacts)

### CodeQL
- `contents: read` (default)
- `security-events: write` (required)

## 📝 Best Practices

✅ **DO**
- Run `dotnet format` before committing
- Run tests locally before pushing
- Keep test execution under 10 minutes per job
- Use meaningful commit messages

❌ **DON'T**
- Push directly to `main` (should be protected)
- Ignore failing tests
- Skip code formatting checks
- Commit secrets or credentials

## 🎯 Success Checklist

Before merging a PR, ensure:
- [ ] All CI jobs are green
- [ ] No new security alerts
- [ ] Architecture tests pass
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] PR template completed
- [ ] Reviews approved


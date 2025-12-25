# Quick Reference: GitHub Actions Workflows

## 📊 Workflows Overview

| Workflow | File | Triggers | Duration | Purpose |
|----------|------|----------|----------|---------|
| **PR Pipeline** | `pr.yml` | Pull Requests | ~8-12 min | Validate code quality, tests, builds |
| **CodeQL Security** | `codeql.yml` | Push, PR, Schedule | ~7-11 min | Security vulnerability scanning |

## 🎯 PR Pipeline (`pr.yml`)

### Quick Stats
- **Jobs**: 9
- **Test Projects**: 6
- **Coverage**: Aggregated across all test types
- **Docker Images**: 2 (Api, ImageProcessor)

### Job Flow
```
code-quality (2-3 min) ──┐
                          │
build (3-5 min) ──────────┼──► architecture-tests (2-3 min) ──┐
                          │                                     │
                          ├──► unit-tests (3-5 min) ───────────┤
                          │                                     │
                          ├──► integration-tests (5-10 min) ───┼──► code-coverage (2-3 min) ──┐
                          │                                     │                              │
                          └──► frontend-tests (2-4 min) ───────┘                              │
                                                                                               │
                          ┌──────────────────────────────────────────────────────────────────┘
                          │
                          ├──► docker-build (5-8 min)
                          │
                          └──► pr-check-summary (1 min)
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

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"

# Build Docker images
docker build -f Nexus.Api/Dockerfile -t nexus-api:local .
docker build -f Nexus.ImageProcessor/Dockerfile -t nexus-processor:local .
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
| `Nexus.Domain.UnitTests` | Unit | None | ~1-2 min |
| `Nexus.Application.UnitTests` | Unit | None | ~1-2 min |
| `Nexus.Api.IntegrationTests` | Integration | PostgreSQL, Redis | ~3-5 min |
| `Nexus.ImageProcessor.IntegrationTests` | Integration | PostgreSQL, Redis | ~2-4 min |
| `Nexus.Migrations.IntegrationTests` | Integration | PostgreSQL | ~1-2 min |
| `Nexus.Frontend.UnitTests` | Unit | Node.js | ~2-4 min |

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
dotnet test --logger "console;verbosity=detailed"

# Run specific failing test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

**Integration Tests**
```bash
# Start services locally
docker-compose up -d postgres redis

# Set connection strings
export ConnectionStrings__Database="Host=localhost;Port=5432;Database=nexus_test;Username=nexus;Password=password"
export ConnectionStrings__Redis="localhost:6379"

# Run tests
dotnet test Nexus.Api.IntegrationTests/Nexus.Api.IntegrationTests.csproj
```

**Docker Build**
```bash
# Test Docker build locally
docker build -f Nexus.Api/Dockerfile -t test:local .
docker build -f Nexus.ImageProcessor/Dockerfile -t test:local .
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

## 📊 Coverage Thresholds

Currently no minimum coverage enforced. Consider adding:

```yaml
- name: Check coverage threshold
  run: |
    coverage=$(grep -oP 'Line coverage: \K[0-9.]+' coverage-report/Summary.txt)
    if (( $(echo "$coverage < 80.0" | bc -l) )); then
      echo "Coverage $coverage% is below 80%"
      exit 1
    fi
```

## ⚡ Performance Tips

1. **Enable NuGet Package Locking**
   ```bash
   dotnet restore --use-lock-file
   ```

2. **Parallel Test Execution**
   Tests already run in parallel via xUnit

3. **Cache Docker Layers**
   Already configured with GitHub Actions cache

4. **Skip Redundant Restores**
   Use `--no-restore` flag (already implemented)

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
- [ ] Code coverage maintained or improved
- [ ] No new security alerts
- [ ] Docker images build successfully
- [ ] Architecture tests pass
- [ ] PR template completed
- [ ] Reviews approved


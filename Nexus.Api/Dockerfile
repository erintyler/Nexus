# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies (leverages layer caching)
COPY ["Nexus.Api/Nexus.Api.csproj", "Nexus.Api/"]
COPY ["Nexus.Application/Nexus.Application.csproj", "Nexus.Application/"]
COPY ["Nexus.Domain/Nexus.Domain.csproj", "Nexus.Domain/"]
COPY ["Nexus.ServiceDefaults/Nexus.ServiceDefaults.csproj", "Nexus.ServiceDefaults/"]
RUN dotnet restore "Nexus.Api/Nexus.Api.csproj"

# Copy source code
COPY . .

# Run Wolverine codegen
WORKDIR "/src/Nexus.Api"
RUN dotnet run -- codegen write

# Publish directly (skip intermediate build step)
RUN dotnet publish "./Nexus.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Create non-root user for better security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D -h /app appuser && \
    chown -R appuser:appgroup /app

# Copy published output
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "Nexus.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Nexus.Frontend/Nexus.Frontend.Client/Nexus.Frontend.Client.csproj", "Nexus.Frontend/Nexus.Frontend.Client/"]
RUN dotnet restore "Nexus.Frontend/Nexus.Frontend.Client/Nexus.Frontend.Client.csproj"
COPY . .
WORKDIR "/src/Nexus.Frontend/Nexus.Frontend.Client"
RUN dotnet build "./Nexus.Frontend.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Nexus.Frontend.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nexus.Frontend.Client.dll"]

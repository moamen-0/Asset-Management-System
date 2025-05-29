# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Use the .NET 9 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AssetManagementSystem.PL/AssetManagementSystem.PL.csproj", "AssetManagementSystem.PL/"]
COPY ["AssetManagementSystem.BLL/AssetManagementSystem.BLL.csproj", "AssetManagementSystem.BLL/"]
COPY ["AssetManagementSystem.DAL/AssetManagementSystem.DAL.csproj", "AssetManagementSystem.DAL/"]

RUN dotnet restore "AssetManagementSystem.PL/AssetManagementSystem.PL.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/AssetManagementSystem.PL"
RUN dotnet build "AssetManagementSystem.PL.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AssetManagementSystem.PL.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - copy the published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for file uploads and logs
RUN mkdir -p /app/wwwroot/files /app/logs && \
    chown -R appuser:appuser /app

# Set environment variables for App Runner
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "AssetManagementSystem.PL.dll"]

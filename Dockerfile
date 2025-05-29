# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

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

# Create directory for file uploads
RUN mkdir -p /app/wwwroot/files

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "AssetManagementSystem.PL.dll"]

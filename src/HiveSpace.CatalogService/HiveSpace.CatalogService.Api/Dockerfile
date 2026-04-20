# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libicu-dev \
    tzdata \
    curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api/HiveSpace.CatalogService.Api.csproj", "src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api/"]
COPY ["src/HiveSpace.CatalogService/HiveSpace.CatalogService.Application/HiveSpace.CatalogService.Application.csproj", "src/HiveSpace.CatalogService/HiveSpace.CatalogService.Application/"]
COPY ["src/HiveSpace.CatalogService/HiveSpace.CatalogService.Domain/HiveSpace.CatalogService.Domain.csproj", "src/HiveSpace.CatalogService/HiveSpace.CatalogService.Domain/"]
COPY ["src/HiveSpace.CatalogService/HiveSpace.CatalogService.Infrastructure/HiveSpace.CatalogService.Infrastructure.csproj", "src/HiveSpace.CatalogService/HiveSpace.CatalogService.Infrastructure/"]
COPY ["libs/HiveSpace.Infrastructure.Messaging.Shared/HiveSpace.Infrastructure.Messaging.Shared.csproj", "libs/HiveSpace.Infrastructure.Messaging.Shared/"]
COPY ["libs/HiveSpace.Infrastructure.Messaging/HiveSpace.Infrastructure.Messaging.csproj", "libs/HiveSpace.Infrastructure.Messaging/"]
COPY ["libs/HiveSpace.Infrastructure.Persistence/HiveSpace.Infrastructure.Persistence.csproj", "libs/HiveSpace.Infrastructure.Persistence/"]
COPY ["libs/HiveSpace.Core/HiveSpace.Core.csproj", "libs/HiveSpace.Core/"]
COPY ["libs/HiveSpace.Domain.Shared/HiveSpace.Domain.Shared.csproj", "libs/HiveSpace.Domain.Shared/"]

RUN dotnet restore "./src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api/HiveSpace.CatalogService.Api.csproj"
COPY . .
WORKDIR "/src/src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api"
RUN dotnet build "./HiveSpace.CatalogService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HiveSpace.CatalogService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HiveSpace.CatalogService.Api.dll"]

# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/HiveSpace.UserService/HiveSpace.UserService.Api/HiveSpace.UserService.Api.csproj", "src/HiveSpace.UserService/HiveSpace.UserService.Api/"]
COPY ["src/HiveSpace.UserService/HiveSpace.UserService.Application/HiveSpace.UserService.Application.csproj", "src/HiveSpace.UserService/HiveSpace.UserService.Application/"]
COPY ["src/HiveSpace.UserService/HiveSpace.UserService.Domain/HiveSpace.UserService.Domain.csproj", "src/HiveSpace.UserService/HiveSpace.UserService.Domain/"]
COPY ["libs/HiveSpace.Domain.Shared/HiveSpace.Domain.Shared.csproj", "libs/HiveSpace.Domain.Shared/"]
COPY ["libs/HiveSpace.Core/HiveSpace.Core.csproj", "libs/HiveSpace.Core/"]
COPY ["src/HiveSpace.UserService/HiveSpace.UserService.Infrastructure/HiveSpace.UserService.Infrastructure.csproj", "src/HiveSpace.UserService/HiveSpace.UserService.Infrastructure/"]
COPY ["libs/HiveSpace.Infrastructure.Persistence/HiveSpace.Infrastructure.Persistence.csproj", "libs/HiveSpace.Infrastructure.Persistence/"]
COPY ["libs/HiveSpace.Infrastructure.Messaging/HiveSpace.Infrastructure.Messaging.csproj", "libs/HiveSpace.Infrastructure.Messaging/"]
COPY ["libs/HiveSpace.Infrastructure.Authorization/HiveSpace.Infrastructure.Authorization.csproj", "libs/HiveSpace.Infrastructure.Authorization/"]
RUN dotnet restore "./src/HiveSpace.UserService/HiveSpace.UserService.Api/HiveSpace.UserService.Api.csproj"
COPY . .
WORKDIR "/src/src/HiveSpace.UserService/HiveSpace.UserService.Api"
RUN dotnet build "./HiveSpace.UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HiveSpace.UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HiveSpace.UserService.Api.dll"]
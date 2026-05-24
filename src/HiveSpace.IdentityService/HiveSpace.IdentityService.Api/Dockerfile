FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/HiveSpace.IdentityService/HiveSpace.IdentityService.Api/HiveSpace.IdentityService.Api.csproj", "src/HiveSpace.IdentityService/HiveSpace.IdentityService.Api/"]
COPY ["src/HiveSpace.IdentityService/HiveSpace.IdentityService.Core/HiveSpace.IdentityService.Core.csproj", "src/HiveSpace.IdentityService/HiveSpace.IdentityService.Core/"]
COPY ["libs/HiveSpace.Core/HiveSpace.Core.csproj", "libs/HiveSpace.Core/"]
COPY ["libs/HiveSpace.Domain.Shared/HiveSpace.Domain.Shared.csproj", "libs/HiveSpace.Domain.Shared/"]
COPY ["libs/HiveSpace.Infrastructure.Authorization/HiveSpace.Infrastructure.Authorization.csproj", "libs/HiveSpace.Infrastructure.Authorization/"]
COPY ["libs/HiveSpace.Infrastructure.Messaging/HiveSpace.Infrastructure.Messaging.csproj", "libs/HiveSpace.Infrastructure.Messaging/"]
COPY ["libs/HiveSpace.Infrastructure.Persistence/HiveSpace.Infrastructure.Persistence.csproj", "libs/HiveSpace.Infrastructure.Persistence/"]
RUN dotnet restore "./src/HiveSpace.IdentityService/HiveSpace.IdentityService.Api/HiveSpace.IdentityService.Api.csproj"
COPY . .
WORKDIR "/src/src/HiveSpace.IdentityService/HiveSpace.IdentityService.Api"
RUN dotnet build "./HiveSpace.IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HiveSpace.IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HiveSpace.IdentityService.Api.dll"]

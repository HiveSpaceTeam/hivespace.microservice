FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/HiveSpace.MediaService/HiveSpace.MediaService.Api/HiveSpace.MediaService.Api.csproj", "src/HiveSpace.MediaService/HiveSpace.MediaService.Api/"]
COPY ["src/HiveSpace.MediaService/HiveSpace.MediaService.Core/HiveSpace.MediaService.Core.csproj", "src/HiveSpace.MediaService/HiveSpace.MediaService.Core/"]
RUN dotnet restore "./src/HiveSpace.MediaService/HiveSpace.MediaService.Api/HiveSpace.MediaService.Api.csproj"
COPY . .
WORKDIR "/src/src/HiveSpace.MediaService/HiveSpace.MediaService.Api"
RUN dotnet build "./HiveSpace.MediaService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HiveSpace.MediaService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HiveSpace.MediaService.Api.dll"]

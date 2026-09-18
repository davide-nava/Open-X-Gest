# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["src/OpenX.Gest.Api/OpenX.Gest.Api.csproj", "src/OpenX.Gest.Api/"]
COPY ["src/OpenX.Gest.Application/OpenX.Gest.Application.csproj", "src/OpenX.Gest.Application/"]
COPY ["src/OpenX.Gest.Domain/OpenX.Gest.Domain.csproj", "src/OpenX.Gest.Domain/"]
COPY ["src/OpenX.Gest.Infrastructure/OpenX.Gest.Infrastructure.csproj", "src/OpenX.Gest.Infrastructure/"]
RUN dotnet restore "./src/OpenX.Gest.Api/OpenX.Gest.Api.csproj"
COPY . .
WORKDIR "/src/src/OpenX.Gest.Api"
RUN dotnet build "./OpenX.Gest.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OpenX.Gest.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenX.Gest.Api.dll"]
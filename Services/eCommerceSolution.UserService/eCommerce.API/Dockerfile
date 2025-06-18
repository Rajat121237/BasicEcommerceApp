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
COPY ["eCommerce.API/eCommerce.API.csproj", "eCommerce.API/"]
RUN dotnet restore "./eCommerce.API/eCommerce.API.csproj"
COPY . .
WORKDIR "/src/eCommerce.API"
RUN dotnet build "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eCommerce.API.dll"]

#docker build -t my-user-app:initial -f ./eCommerce.API/Dockerfile .
#docker run -p 8080:8080 -p 8081:8081 my-user-app:initial

#Technique -> Benefit -> When to Use
#Layer Caching      ->	Faster builds, less network usage											->	Always, especially with Docker
#Dependency Caching ->	Speeds up package restore													->	Always, for .NET, NPM, etc.
#Multi-Stage        ->	Builds Smaller, more secure images(only runtime assets in the final image). ->	Always with Docker

#How Layer Caching Works ?
#Docker checks each line (instruction) in your Dockerfile.
#If nothing has changed in that line and all previous layers, it reuses the cached layer.
#If something changes (like a file is updated or an ARG value changes), Docker invalidates the cache for that step and all subsequent layers.

#docker run mcr.microsoft.com/dotnet/sdk:8.0 cat /etc/os-release
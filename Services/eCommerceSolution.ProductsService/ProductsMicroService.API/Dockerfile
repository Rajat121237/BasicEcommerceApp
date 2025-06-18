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
COPY ["ProductsMicroService.API/ProductsMicroService.API.csproj", "ProductsMicroService.API/"]
RUN dotnet restore "./ProductsMicroService.API/ProductsMicroService.API.csproj"
COPY . .
WORKDIR "/src/ProductsMicroService.API"
RUN dotnet build "./ProductsMicroService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductsMicroService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MYSQL_HOST=localhost
ENV MYSQL_PASSWORD=admin
ENTRYPOINT ["dotnet", "ProductsMicroService.API.dll"]

# When building Image & running the container initially. Throws error because the database container is not available yet.
# docker build -t my-product-app:initial -f ./ProductsMicroService.API/Dockerfile .
# docker run -p 8080:8080 -p 8081:8081 my-product-app:initial

# Before pulling & running mysql container, Need to create a network for the product microservice to communicate with the mysql container.
# docker network create productmicroservice-network
# docker network ls
# docker network inspect productmicroservice-network

# Added both the containers to the same network so that they can communicate with each other. 
# This also throws error on running the app because connectionstring contains hardcoded value of hostname as localhost and password as admin.
# Since the both the container are isolated and password should not be hardcoded, we use environment variables to pass the values.
# docker run -it -p 3307:3306 --network=productmicroservice-network mysql:latest
# docker run -p 8080:8080 -p 8081:8081 --network=productmicroservice-network my-product-app:initial

# Now app successfully connected to database but the database does not exist. 
# Therefore, we need to map the container to a volume or mount a directory with the database initialization scripts.
# docker run -it -e MYSQL_ROOT_PASSWORD=admin -p 3307:3306 --hostname=mysql-host-productsmicroservice --network=productmicroservice-network mysql:latest
# docker run -p 8080:8080 -p 8081:8081 --network=productmicroservice-network my-product-app:initial

# Great, App is running successfully now.
# docker run -it -e MYSQL_ROOT_PASSWORD=admin -p 3307:3306 --hostname=mysql-host-productsmicroservice --network=productmicroservice-network -v D:/Microservices/eCommerceSolution.ProductsService/ProductsMicroService.API/mysql-init:/docker-entrypoint-initdb.d mysql:latest
# docker run -p 8080:8080 -p 8081:8081 --network=productmicroservice-network -e MYSQL_HOST=mysql-host-productsmicroservice -e MYSQL_PASSWORD=admin my-product-app:initial

# docker build -t products-microservice:1.0 -f ./ProductsMicroService.API/Dockerfile .
# docker login --username rajatpandey121237 --password <your-password>
# docker tag products-microservice:1.0 rajatpandey121237/products-microservice:v1.0
# docker push rajatpandey121237/products-microservice:v1.0

# docker run -p 8080:8080 -p 8081:8081 --network=productmicroservice-network -e MYSQL_HOST=mysql-host-productsmicroservice -e MYSQL_PASSWORD=admin rajatpandey121237/products-microservice:v1.0
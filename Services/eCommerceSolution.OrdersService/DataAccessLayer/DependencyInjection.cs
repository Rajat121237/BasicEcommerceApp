﻿using eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrderMicroservice.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //TO DO: Add data access layer services into the IoC container
        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;

        string connectionString = connectionStringTemplate
            .Replace("$MONGODB_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
            .Replace("$MONGODB_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));

        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped<IMongoDatabase>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();
        return services;
    }
}
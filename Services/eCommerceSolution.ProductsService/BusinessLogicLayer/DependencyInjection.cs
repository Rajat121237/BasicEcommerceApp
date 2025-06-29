
using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        //TODO: Register your business logic layer services here.
        services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();
        return services;
    }
}

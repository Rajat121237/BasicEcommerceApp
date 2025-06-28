using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroMicroservicePolicies : IProductsMicroMicroservicePolicies
{
    private readonly ILogger<ProductsMicroMicroservicePolicies> _logger;
    public ProductsMicroMicroservicePolicies(ILogger<ProductsMicroMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .FallbackAsync( async (context) =>
            {
                _logger.LogWarning("Fallback triggered: The request failed returning dummy data");
                ProductDTO product = new ProductDTO()
                {
                    ProductID = Guid.Empty,
                    ProductName = "Temporarily UnAvailable(Fallback)",
                    Category = "Temporarily UnAvailable(Fallback)",
                    UnitPrice = 0,
                    QuantityInStock = 0
                };

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(JsonSerializer.Serialize(product), encoding: Encoding.UTF8, "application/json")
                };

                return response;
            });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 2, //Allow only 2 concurrent requests
            maxQueuingActions: 40, //Allow 40 requests to be queued
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("Bulkhead isolation policy triggered: Too many requests, rejecting further requests.");
                throw new BulkheadRejectedException("Bulkhead queue is full");
            });
        return policy;
    }
}

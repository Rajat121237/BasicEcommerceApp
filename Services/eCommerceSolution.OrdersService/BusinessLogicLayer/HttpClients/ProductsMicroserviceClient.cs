using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Polly.Bulkhead;

namespace BusinessLogicLayer.HttpClients;
public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try
        {
            string cacheKey = $"product:{productID}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);
            if (cachedProduct != null)
            {
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO?>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFromFallback = await response.Content.ReadFromJsonAsync<ProductDTO>();
                    if (productFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented");
                    }
                    return productFromFallback;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                }
            }

            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null)
            {
                throw new ArgumentException("Invalid ProductID");
            }

            //Save Product in the cache
            string productJson = JsonSerializer.Serialize(product);
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60)).SetSlidingExpiration(TimeSpan.FromSeconds(30));
            await _distributedCache.SetStringAsync(cacheKey, productJson, cacheOptions);

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogInformation("Bulkhead isolation policy triggered, returning a temporary product");
            return new ProductDTO(Guid.Empty, "Temporarily UnAvailable(Bulkhead)", "Temporarily UnAvailable(Bulkhead)", 0, 0);
        }
        
    }
}

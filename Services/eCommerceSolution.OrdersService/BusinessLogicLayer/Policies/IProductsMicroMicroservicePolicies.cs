using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductsMicroMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();
}

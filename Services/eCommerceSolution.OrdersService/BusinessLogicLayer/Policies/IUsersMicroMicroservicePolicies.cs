using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}

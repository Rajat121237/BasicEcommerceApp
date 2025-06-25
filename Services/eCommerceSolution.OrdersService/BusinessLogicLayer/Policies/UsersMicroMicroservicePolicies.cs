using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
public class UsersMicroMicroservicePolicies : IUsersMicroMicroservicePolicies
{
    private readonly ILogger<UsersMicroMicroservicePolicies> _logger;
    private readonly IPollyPolicies _pollyPolicies;
    public UsersMicroMicroservicePolicies(ILogger<UsersMicroMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }
    
    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromMilliseconds(1500));
        AsyncPolicyWrap<HttpResponseMessage> combinedPolicy =  Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return combinedPolicy;
    }
}

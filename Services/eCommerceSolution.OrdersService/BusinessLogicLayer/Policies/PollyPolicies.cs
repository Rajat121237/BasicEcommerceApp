using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroMicroservicePolicies> _logger;
    public PollyPolicies(ILogger<UsersMicroMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage>  retryPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount: retryCount, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                _logger.LogInformation($"Retry {retryCount} due to {outcome.Result.StatusCode} at {DateTime.UtcNow}. Retrying in {timespan.TotalSeconds} seconds.");
            });
        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, durationOfBreak,
            onBreak: (outcome, timespan) =>
            {
                _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes");
            },
            onReset: () =>
            {
                _logger.LogInformation("Circuit breaker reset, allowing requests to pass through again.");
            });
        return circuitPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        return timeoutPolicy;
    }
}

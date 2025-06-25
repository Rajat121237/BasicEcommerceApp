using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
public class UsersMicroMicroservicePolicies : IUsersMicroMicroservicePolicies
{
    private readonly ILogger<UsersMicroMicroservicePolicies> _logger;
    public UsersMicroMicroservicePolicies(ILogger<UsersMicroMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage>  retryPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                _logger.LogInformation($"Retry {retryCount} due to {outcome.Result.StatusCode} at {DateTime.UtcNow}. Retrying in {timespan.TotalSeconds} seconds.");
            });
        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromMinutes(2),
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
}

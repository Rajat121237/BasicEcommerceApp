using DnsClient.Internal;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            string cacheKey = $"user:{userID}";
            string? cachedUser = await _distributedCache.GetStringAsync(cacheKey);
            if (cachedUser != null)
            {
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO?>(cachedUser);
                return userFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO>();
                    if (userFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented");
                    }
                    return userFromFallback;
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
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    //Below return statement is used to return a fallback data in case of retry policy.
                    return new UserDTO(Guid.Empty, "Temporarily UnAvailable", "Temporarily UnAvailable", "Temporarily UnAvailable");
                }
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            string userJson = JsonSerializer.Serialize(user);
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60)).SetSlidingExpiration(TimeSpan.FromSeconds(30));
            await _distributedCache.SetStringAsync(cacheKey, userJson, cacheOptions);
            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogInformation("Circuit breaker is open, returning a temporary user");
            return new UserDTO(Guid.Empty, "Temporarily UnAvailable(BrokenCircuit)", "Temporarily UnAvailable(BrokenCircuit)", "Temporarily UnAvailable(BrokenCircuit)");
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogInformation("Timeout occured while fetching user data, returning a temporary user");
            return new UserDTO(Guid.Empty, "Temporarily UnAvailable(Timeout)", "Temporarily UnAvailable(Timeout)", "Temporarily UnAvailable(Timeout)");
        }
    }
}

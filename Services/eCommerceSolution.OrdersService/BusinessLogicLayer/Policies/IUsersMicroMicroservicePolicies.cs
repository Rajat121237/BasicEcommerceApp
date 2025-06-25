using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}

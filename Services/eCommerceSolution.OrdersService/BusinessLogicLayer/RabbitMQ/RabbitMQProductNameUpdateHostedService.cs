using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _rabbitMQProductNameUpdateConsumer;
    public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer rabbitMQProductNameUpdateConsumer)
    {
        _rabbitMQProductNameUpdateConsumer = rabbitMQProductNameUpdateConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMQProductNameUpdateConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQProductNameUpdateConsumer.Dispose();
        return Task.CompletedTask;
    }
}

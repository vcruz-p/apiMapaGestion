using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Events;

public class RedisEventBus : IEventBus
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventBus> _logger;

    public RedisEventBus(IConnectionMultiplexer redis, ILogger<RedisEventBus> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
            _logger.LogInformation("Published event to channel {Channel}: {Message}", channel, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to channel {Channel}", channel);
        }
    }

    public async Task SubscribeAsync(string channel, Func<string, Task> handler, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(RedisChannel.Literal(channel), async (ch, message) =>
            {
                try
                {
                    await handler(message.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event from channel {Channel}", ch);
                }
            });
            _logger.LogInformation("Subscribed to channel {Channel}", channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to channel {Channel}", channel);
        }
    }
}

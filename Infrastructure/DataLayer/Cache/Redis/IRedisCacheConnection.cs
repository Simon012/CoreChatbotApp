using StackExchange.Redis;

namespace CoreChatbotApp.Infrastructure.DataLayer.Cache.Redis
{
    public interface IRedisCacheConnection
    {
        ConnectionMultiplexer Connection { get; }
    }
}

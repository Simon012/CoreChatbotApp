using CoreChatbotApp.Utilities.Configurations;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

namespace CoreChatbotApp.Infrastructure.DataLayer.Cache.Redis
{
    public class RedisCacheConnection : IRedisCacheConnection
    {
        private ConnectionMultiplexer _connection;

        public RedisCacheConnection(IOptions<CacheConfig> options)
        {
            _connection = ConnectionMultiplexer.Connect(options.Value.RedisCacheConnectionString);
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                if (!_connection.IsConnected)
                {
                    _connection = ConnectionMultiplexer.Connect(_connection.Configuration);
                }
                return _connection;
            }
        }
    }
}

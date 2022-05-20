using ConnorWebSockets.Bases;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Connor.Messaging.Bases
{
    public abstract class UserCacheBase<T> : SocketCacheBase<T, long> where T : UserWebSocketBase
    {
        protected UserCacheBase(IConnectionMultiplexer connectionMultiplexer, ILogger<SocketCacheBase<T, long>> logger) : base(connectionMultiplexer, logger)
        {
        }
    }
}

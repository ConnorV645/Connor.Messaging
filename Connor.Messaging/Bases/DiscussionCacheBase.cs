using ConnorWebSockets.Bases;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Connor.Messaging.Bases
{
    public abstract class DiscussionCacheBase<T> : SocketCacheBase<T, long> where T : UserWebSocketBase
    {
        public DiscussionCacheBase(IConnectionMultiplexer connectionMultiplexer, ILogger<SocketCacheBase<T, long>> logger) : base(connectionMultiplexer, logger)
        {

        }
    }
}

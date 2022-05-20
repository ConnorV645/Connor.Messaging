using ConnorWebSockets.Bases;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connor.Messaging.Bases
{
    public abstract class DiscussionCacheBase<T> : SocketCacheBase<T, long> where T : WebSocketBase
    {
        public DiscussionCacheBase(IConnectionMultiplexer connectionMultiplexer, ILogger<SocketCacheBase<T, long>> logger) : base(connectionMultiplexer, logger)
        {

        }

        public async Task PushToUsers(List<long> userIds, string message)
        {
            foreach (var id in userIds)
            {
                await PushToUser(id, message);
            }
        }

        public async Task PushToUser(long userId, string message)
        {
            await connectionMultiplexer.GetSubscriber().PublishAsync(GetUserKey(userId), message);
        }

        public abstract string GetUserKey(long userId);
    }
}

using Connor.Messaging.Bases;
using System;
using System.Threading.Tasks;

namespace Connor.Messaging.Interfaces
{
    public interface IRequestHandler<T, R, C, U>
        where T : UserWebSocketBase
        where R : Enum
        where C : DiscussionCacheBase<T>
        where U : UserCacheBase<T>
    {
        Task<object> HandleRequest(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C, U> handler);
    }
}

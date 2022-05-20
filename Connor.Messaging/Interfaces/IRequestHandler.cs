using Connor.Messaging.Bases;
using ConnorWebSockets.Bases;
using System;
using System.Threading.Tasks;

namespace Connor.Messaging.Interfaces
{
    public interface IRequestHandler<T, R, C> where T : WebSocketBase where R : Enum where C : DiscussionCacheBase<T>
    {
        Task<object> HandleRequest(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C> handler);
    }
}

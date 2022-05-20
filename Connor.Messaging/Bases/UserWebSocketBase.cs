using ConnorWebSockets.Bases;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Connor.Messaging.Bases
{
    public class UserWebSocketBase : WebSocketBase
    {
        public long UserId { get; set; }
        public List<long> OpenDiscussions { get; set; } = new();
        public UserWebSocketBase(WebSocket socket) : base(socket)
        {
        }
    }
}

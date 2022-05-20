using Connor.Messaging.Enums;
using Connor.Messaging.Exceptions;
using Connor.Messaging.Interfaces;
using ConnorWebSockets.Bases;
using ConnorWebSockets.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Connor.Messaging.Bases
{
    public abstract class MessageHandlerBase<T, R, C> : WebSocketHandlerBase<T> where T : WebSocketBase where R : Enum where C : DiscussionCacheBase<T>
    {
        protected readonly IServiceScopeFactory factory;

        public MessageHandlerBase(IServiceScopeFactory factory, IConnectionManager<T> webSocketConnectionManager, ILogger logger) : base(webSocketConnectionManager, logger)
        {
            this.factory = factory;
        }

        public override async Task<string> OnConnected(T socket)
        {
            var socketId = await base.OnConnected(socket);

            // Log Socket ID connection
            return socketId;
        }

        public override async Task OnDisconnected(T socket)
        {
            try
            {
                await base.OnDisconnected(socket);
            }
            catch (WebSocketException)
            {
                // Do Nothing
            }
        }

        public abstract IRequestHandler<T, R, C> GetRequestHandler(R requestType, IServiceProvider serviceProvider);
        public abstract bool IsPublicRequest(R requestType);
        public abstract bool IsHeartBeat(R requestType);

        public override async Task ReceiveAsync(T socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var request = JsonConvert.DeserializeObject<SocketRequestBase<R>>(message);
            object response;
            if (IsHeartBeat(request.RequestType))
            {
                return;
            }

            if (!socket.IsAuthorized && !IsPublicRequest(request.RequestType))
            {
                response = new ResponseBase<R> { IsError = true, ErrorCode = ErrorCode.Unauthorized, RequestType = request.RequestType };
            }
            else if (request.Data == null)
            {
                response = new ResponseBase<R> { IsError = true, ErrorCode = ErrorCode.BadRequest, ErrorMessage = "Data Object Required", RequestType = request.RequestType };
            }
            else
            {
                try
                {
                    using var scope = factory.CreateScope();
                    response = await GetRequestHandler(request.RequestType, scope.ServiceProvider).HandleRequest(request, socket, this);
                }
                catch (Exception ex)
                {
                    var error = new ResponseBase<R> { IsError = true, RequestType = request.RequestType, ErrorMessage = ex.Message };
                    if (ex is BadRequestException)
                    {
                        error.ErrorCode = ErrorCode.BadRequest;
                    }
                    else if (ex is UnauthorizedAccessException)
                    {
                        error.ErrorCode = ErrorCode.Unauthorized;
                    }
                    else if (ex is UnknownException)
                    {
                        error.ErrorCode = ErrorCode.Unknown;
                    }
                    else if (ex is ForbidException)
                    {
                        Logger.LogWarning(ex, "Receive Socket Error");
                        error.ErrorCode = ErrorCode.Forbidden;
                    }
                    else
                    {
                        Logger.LogError(ex, "Receive Socket Error");
                        error.ErrorCode = ErrorCode.ServerError;
                    }
                    response = error;
                }
            }

            // Respond
            if (response != null)
            {
                await SendMessageAsync(socket, JsonConvert.SerializeObject(response, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));
            }
        }

        public override async Task ReceiveBinaryAsync(T socket, WebSocketReceiveResult result, byte[] buffer)
        {
            return;
        }
    }
}

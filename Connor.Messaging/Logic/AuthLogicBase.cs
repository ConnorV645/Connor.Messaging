using Connor.Messaging.Bases;
using Connor.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Connor.Messaging.Logic
{
    public abstract class AuthLogicBase<T, R, C, U> : IRequestHandler<T, R, C, U>
        where T : UserWebSocketBase
        where R : Enum
        where C : DiscussionCacheBase<T>
        where U : UserCacheBase<T>
    {
        protected readonly ILogger logger;
        protected readonly C discussionCache;
        private readonly U userCache;

        public AuthLogicBase(ILogger logger, C discussionCache, U userCache)
        {
            this.logger = logger;
            this.discussionCache = discussionCache;
            this.userCache = userCache;
        }

        #region Authorization
        public abstract bool IsAuthRequest(R requestType);
        public abstract Task<(bool authorized, long userId)> IsAuthorized(SocketRequestBase<R> request);
        public abstract IResponse<R> GetAuthResponse();
        #endregion

        #region Close Discussion
        public abstract bool IsCloseDiscussionRequest(R requestType);
        #endregion

        public async Task<object> HandleRequest(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C, U> handler)
        {
            if (IsAuthRequest(request.RequestType))
            {
                return await HandleAuthorization(request, socket, handler);
            }
            else if (!socket.IsAuthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else if (IsCloseDiscussionRequest(request.RequestType))
            {
                return await HandleCloseDiscussion(request, socket);
            }

            return new ResponseBase<R> { RequestType = request.RequestType, ErrorCode = Enums.ErrorCode.Unknown, ErrorMessage = "Unknown Request Type", IsError = true };
        }

        public async Task<IResponse<R>> HandleAuthorization(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C, U> handler)
        {
            // Check authorization
            var (authorized, userId) = await IsAuthorized(request);
            if (!authorized)
            {
                throw new UnauthorizedAccessException();
            }

            var response = GetAuthResponse();
            try
            {
                socket.IsAuthorized = authorized;
                socket.UserId = userId;

                await userCache.SubscribeToChannel(socket, socket.UserId, async (message, _, sockets) =>
                {
                    foreach (var socket in sockets)
                    {
                        try
                        {
                            var msg = JsonConvert.DeserializeObject<IHasSourceUser>(message);
                            if (msg.UserId == socket.UserId)
                            {
                                continue;
                            }
                        }
                        catch { }
                        await handler.SendMessageAsync(socket, message);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Handling Authorization");
                response.IsError = true;
                response.ErrorCode = Enums.ErrorCode.ServerError;
                response.ErrorMessage = "Error Handling Authorization";
            }

            return response;
        }

        public async Task<IResponse<R>> HandleCloseDiscussion(SocketRequestBase<R> request, T socket)
        {
            try
            {
                var msg = request.Data.ToObject<ICloseDiscussion>();
                socket.OpenDiscussions.Remove(msg.DiscussionId);
                await discussionCache.UnsubscribeFromChannel(socket, msg.DiscussionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Handling Authorization");
            }

            return null;
        }
    }
}

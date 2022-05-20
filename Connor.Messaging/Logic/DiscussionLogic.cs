using Connor.Messaging.Bases;
using Connor.Messaging.Interfaces;
using ConnorWebSockets.Bases;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connor.Messaging.Logic
{
    public abstract class DiscussionLogic<T, R, C> : IRequestHandler<T, R, C> where T : WebSocketBase where R : Enum where C : DiscussionCacheBase<T>
    {
        protected readonly ILogger logger;
        protected readonly C discussionCache;

        public DiscussionLogic(ILogger logger, C discussionCache)
        {
            this.logger = logger;
            this.discussionCache = discussionCache;
        }
        #region Add Message
        public abstract bool IsAddMessage(R requestType);
        public abstract IResponse<R> GetAddResponse();
        public abstract Task<long> CreateMessage(IAddMessage message);
        public abstract IAddMessage DeserializeAddMessageRequest(JToken data, T socket);
        public abstract IMessageItem GetMessageItem(IAddMessage message, long messageId);
        public abstract List<long> SendToUnsubscribedOnlineUsers(long dicussionId, T socket);
        public abstract Task HandlePushNotifications(long discussionId);
        public abstract void AddResponseMsg(IResponse<R> response, string messageJSON);
        #endregion

        #region Load Messages
        public abstract bool IsLoadMessage(R requestType);
        public abstract IResponse<R> GetLoadResponse();
        public abstract ILoadMessage DeserializeLoadMessageRequest(JToken data, T Socket);
        public abstract Task<string> GetLoadMessageData(ILoadMessage message);
        public abstract void LoadResponseMsg(IResponse<R> response, string messageJSON);
        #endregion

        #region Typing Status
        public abstract bool IsTypingStatus(R requestType);
        public abstract bool IsTypingDoneStatus(R requestType);
        #endregion

        public async Task<object> HandleRequest(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C> handler)
        {
            if (!socket.IsAuthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (IsAddMessage(request.RequestType))
            {
                return await AddMessage(request, socket);
            }
            else if (IsLoadMessage(request.RequestType))
            {
                return await LoadMessages(request, socket);
            }

            return new ResponseBase<R> { RequestType = request.RequestType, ErrorCode = Enums.ErrorCode.Unknown, ErrorMessage = "Unknown Request Type", IsError = true };
        }

        public async Task<IResponse<R>> AddMessage(SocketRequestBase<R> request, T socket)
        {
            var response = GetAddResponse();
            response.RequestType = request.RequestType;

            try
            {
                // Convert the Request
                var message = DeserializeAddMessageRequest(request.Data, socket);
                // Record in your DB
                var messageId = await CreateMessage(message);
                // Convert Item to something to distribute
                var msgJSON = JsonConvert.SerializeObject(GetMessageItem(message, messageId));
                // Send Item to everyone subscribed
                await discussionCache.PublishMessage(message.DiscussionId, msgJSON);
                // Send Item to online members who arent subscribed
                var usersToAlsoSendTo = SendToUnsubscribedOnlineUsers(message.DiscussionId, socket);
                if (usersToAlsoSendTo.Count > 0)
                {
                    await discussionCache.PushToUsers(usersToAlsoSendTo, msgJSON);
                }
                // Send Push Notifications to offline users if applicable
                await HandlePushNotifications(message.DiscussionId);
                // Add JSON object to the response
                AddResponseMsg(response, msgJSON);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Adding Message");
                response.IsError = true;
                response.ErrorCode = Enums.ErrorCode.ServerError;
                response.ErrorMessage = "Error Sending Message";
            }

            return response;
        }

        public async Task<IResponse<R>> LoadMessages(SocketRequestBase<R> request, T socket)
        {
            var response = GetLoadResponse();
            response.RequestType = request.RequestType;

            try
            {
                // Convert the Request
                var message = DeserializeLoadMessageRequest(request.Data, socket);
                // Fetch The Data
                var jsonData = await GetLoadMessageData(message);
                // Add Data To The Response
                LoadResponseMsg(response, jsonData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Loading Message");
                response.IsError = true;
                response.ErrorCode = Enums.ErrorCode.ServerError;
                response.ErrorMessage = "Error Loading Messages";
            }

            return response;
        }
    }
}

using Connor.Messaging.Bases;
using Connor.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connor.Messaging.Logic
{
    public abstract class DiscussionLogicBase<T, R, C, U> : IRequestHandler<T, R, C, U>
        where T : UserWebSocketBase
        where R : Enum
        where C : DiscussionCacheBase<T>
        where U : UserCacheBase<T>
    {
        protected readonly ILogger logger;
        protected readonly C discussionCache;
        protected readonly U userCache;

        public DiscussionLogicBase(ILogger logger, C discussionCache, U userCache)
        {
            this.logger = logger;
            this.discussionCache = discussionCache;
            this.userCache = userCache;
        }

        #region Add Message
        public abstract bool IsAddMessage(R requestType);
        public abstract IResponse<R> GetAddResponse();
        public abstract Task<long> CreateMessage(IAddMessage message);
        public abstract IAddMessage DeserializeAddMessageRequest(JToken data, T socket);
        public abstract IMessageItem GetMessageItem(IAddMessage message, long messageId);
        public abstract List<long> SendToUnsubscribedOnlineUsers(long dicussionId, T socket);
        public abstract Task HandlePushNotifications(long discussionId, long senderUserId);
        public abstract void AddResponseMsg(IResponse<R> response, string messageJSON);
        public abstract IReadMessage GetReadMessage(IMessageItem message, long userId);
        #endregion

        #region Load Messages
        public abstract bool IsLoadMessage(R requestType);
        public abstract IResponse<R> GetLoadResponse();
        public abstract ILoadMessage DeserializeLoadMessageRequest(JToken data, T Socket);
        public abstract Task<string> GetLoadMessageData(ILoadMessage message);
        public abstract void LoadResponseMsg(IResponse<R> response, string messageJSON);
        public abstract Task UpdateLastRead(long discussionId, long userId);
        #endregion

        #region Typing Status
        public abstract bool IsTypingStatus(R requestType);
        public abstract IResponse<R> GetTypingResponse(SocketRequestBase<R> request);
        public abstract ITypingMessage GetTypingMessage(SocketRequestBase<R> request, T socket);
        #endregion

        public async Task<object> HandleRequest(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C, U> handler)
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
                return await LoadMessages(request, socket, handler);
            }
            else if (IsTypingStatus(request.RequestType))
            {
                return await UpdateTypingStatus(request, socket);
            }

            return new ResponseBase<R> { RequestType = request.RequestType, ErrorCode = Enums.ErrorCode.Unknown, ErrorMessage = "Unknown Request Type", IsError = true };
        }

        public async Task<IResponse<R>> AddMessage(SocketRequestBase<R> request, T socket)
        {
            var response = GetAddResponse();
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
                    await userCache.PublishMessagesToMultiple(usersToAlsoSendTo, msgJSON);
                }
                // Send Push Notifications to offline users if applicable
                await HandlePushNotifications(message.DiscussionId, socket.UserId);
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

        public async Task<IResponse<R>> LoadMessages(SocketRequestBase<R> request, T socket, MessageHandlerBase<T, R, C, U> handler)
        {
            var response = GetLoadResponse();

            try
            {
                // Convert the Request
                var message = DeserializeLoadMessageRequest(request.Data, socket);
                // Fetch The Data
                var jsonData = await GetLoadMessageData(message);
                // Mark as Read
                await UpdateLastRead(message.DiscussionId, message.UserId);
                // Add Data To The Response
                LoadResponseMsg(response, jsonData);

                if (message.Skip == 0 && !socket.OpenDiscussions.Contains(message.DiscussionId))
                {
                    socket.OpenDiscussions.Add(message.DiscussionId);
                    await discussionCache.SubscribeToChannel(socket, message.DiscussionId, async (message, _, sockets) =>
                    {
                        foreach (var socket in sockets)
                        {
                            try
                            {
                                var msg = JsonConvert.DeserializeObject(message);
                                if (msg is IHasSourceUser msgWithUser)
                                {
                                    if (msgWithUser.UserId == socket.UserId)
                                    {
                                        continue;
                                    }

                                    if (msg is IMessageItem sentMessage)
                                    {
                                        // Update Read Status
                                        var readMsg = GetReadMessage(sentMessage, socket.UserId);
                                        await discussionCache.PublishMessage(sentMessage.DiscussionId, JsonConvert.SerializeObject(readMsg));
                                    }
                                }
                            }
                            catch { }
                            await handler.SendMessageAsync(socket, message);
                        }
                    });
                }
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

        public async Task<IResponse<R>> UpdateTypingStatus(SocketRequestBase<R> request, T socket)
        {
            try
            {
                // Get Distributable Message
                var message = GetTypingMessage(request, socket);
                // JSON Convert
                var json = JsonConvert.SerializeObject(message);
                // Send to Subscribed Users
                await discussionCache.PublishMessage(message.DiscussionId, json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Updating Typing");
                var response = GetTypingResponse(request);
                response.IsError = true;
                response.ErrorCode = Enums.ErrorCode.ServerError;
                response.ErrorMessage = "Error Updating Typing";
                return response;
            }
            // No need to write back to sender
            return null;
        }
    }
}

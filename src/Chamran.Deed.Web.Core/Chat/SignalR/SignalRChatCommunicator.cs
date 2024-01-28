using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Notifications;
using Abp.ObjectMapping;
using Abp.RealTime;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using Chamran.Deed.Authorization.Users;
using Microsoft.AspNetCore.SignalR;
using Chamran.Deed.Chat;
using Chamran.Deed.Chat.Dto;
using Chamran.Deed.Friendships;
using Chamran.Deed.Friendships.Dto;
using Chamran.Deed.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Chamran.Deed.Web.Chat.SignalR
{
    public class SignalRChatCommunicator : IChatCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private readonly IObjectMapper _objectMapper;

        private readonly IHubContext<ChatHub> _chatHub;

        private readonly INotificationPublisher _notificationPublisher;
        private readonly UserManager _userManager;

        public SignalRChatCommunicator(
            IObjectMapper objectMapper,
            IHubContext<ChatHub> chatHub,
            INotificationPublisher notificationPublisher,
            UserManager userManager)
        {
            _objectMapper = objectMapper;
            _chatHub = chatHub;
            _notificationPublisher = notificationPublisher;
            _userManager = userManager;
            Logger = NullLogger.Instance;
        }

        public async Task DeleteMessageToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, Guid sharedMessageId)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("deleteChatMessage", sharedMessageId);
            }
        }

        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }
                var dto = _objectMapper.Map<ChatMessageDto>(message);
                try
                {
                    if (client.UserId != null)
                    {
                        //var receiver = new UserIdentifier(1, client.UserId.Value);
                        var receiver = new UserIdentifier(1, message.UserId);
                        var friendUser = await _userManager.GetUserAsync(receiver);
                        dto.FriendName = friendUser.Name;
                        dto.FriendSurName = friendUser.Surname;
                        dto.FriendProfilePictureId = friendUser.ProfilePictureId;
                    }
                }
                catch (Exception)
                {
                    //ignored
                }
                await signalRClient.SendAsync("getChatMessage", dto);
              
            }
        }

        public async Task EditMessageToClient(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, Guid sharedMessageId, string message)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("editChatMessage", sharedMessageId, message);
            }
        }


        public async Task SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friendship, bool isOwnRequest, bool isFriendOnline)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                var friendshipRequest = _objectMapper.Map<FriendDto>(friendship);
                friendshipRequest.IsOnline = isFriendOnline;

                await signalRClient.SendAsync("getFriendshipRequest", friendshipRequest, isOwnRequest);
            }
        }

        public async Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getUserConnectNotification", user, isConnected);
            }
        }

        public async Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getUserStateChange", user, newState);
            }
        }

        public async Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getallUnreadMessagesOfUserRead", user);
            }
        }

        public async Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getReadStateChange", user);
            }
        }

        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _chatHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }
    }
}
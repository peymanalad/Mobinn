﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;
using Chamran.Deed.Friendships;

namespace Chamran.Deed.Chat
{
    public interface IChatCommunicator
    {
        Task DeleteMessageToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, Guid sharedMessageId);

        Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message);
        Task EditMessageToClient(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, Guid sharedMessageId, string message);

        Task SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friend, bool isOwnRequest, bool isFriendOnline);

        Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected);

        Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState);

        Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user);

        Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> onlineFriendClients, UserIdentifier user);
    }
}
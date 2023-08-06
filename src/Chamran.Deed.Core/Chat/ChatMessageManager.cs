using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Common;
using Chamran.Deed.Friendships;
using Chamran.Deed.Friendships.Cache;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Chat
{
    [AbpAuthorize]
    public class ChatMessageManager : DeedDomainServiceBase, IChatMessageManager
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;
        private readonly UserManager _userManager;
        private readonly ITenantCache _tenantCache;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IUserEmailer _userEmailer;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<BinaryObject,Guid> _binaryObjectRepository;

        public ChatMessageManager(
            IFriendshipManager friendshipManager,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager<ChatChannel> onlineClientManager,
            UserManager userManager,
            ITenantCache tenantCache,
            IUserFriendsCache userFriendsCache,
            IUserEmailer userEmailer,
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatFeatureChecker chatFeatureChecker,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<BinaryObject,Guid> binaryObjectRepository)
        {
            _friendshipManager = friendshipManager;
            _chatCommunicator = chatCommunicator;
            _onlineClientManager = onlineClientManager;
            _userManager = userManager;
            _tenantCache = tenantCache;
            _userFriendsCache = userFriendsCache;
            _userEmailer = userEmailer;
            _chatMessageRepository = chatMessageRepository;
            _chatFeatureChecker = chatFeatureChecker;
            _unitOfWorkManager = unitOfWorkManager;
            _binaryObjectRepository = binaryObjectRepository;
        }

        public async Task DeleteMessageAsync(UserIdentifier sender, int messageId)
        {

            //await _chatMessageRepository.DeleteAsync(x => x.Id == messageId);
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await _chatMessageRepository.DeleteAsync(messageId);
                var clients = _onlineClientManager.GetAllByUserId(sender);
                await _chatCommunicator.DeleteMessageToClients(clients, sender, messageId);
            });

        }

        public async Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId)
        {
            CheckReceiverExists(receiver);

            _chatFeatureChecker.CheckChatFeatures(sender.TenantId, receiver.TenantId);

            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(sender, receiver))?.State;
            if (friendshipState == FriendshipState.Blocked)
            {
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sharedMessageId = Guid.NewGuid();

            await HandleSenderToReceiverAsync(sender, receiver, message, sharedMessageId);
            await HandleReceiverToSenderAsync(sender, receiver, message, sharedMessageId);
            await HandleSenderUserInfoChangeAsync(sender, receiver, senderTenancyName, senderUserName, senderProfilePictureId);
        }

        private void CheckReceiverExists(UserIdentifier receiver)
        {
            var receiverUser = _userManager.GetUserOrNull(receiver);
            if (receiverUser == null)
            {
                throw new UserFriendlyException(L("TargetUserNotFoundProbablyDeleted"));
            }
        }

        public virtual long Save(ChatMessage message)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    var id= _chatMessageRepository.InsertAndGetId(message);
                    var fileId=GetIdFromMessage(message.Message);
                    if (fileId != null)
                    {
                        var binaryFile=_binaryObjectRepository.Get(fileId.Value);
                        binaryFile.SourceType=(int?)BinarySourceType.ChatFile;
                        binaryFile.SourceGuid= fileId;

                    }
                    return id;
                }
            });
        }

        private Guid? GetIdFromMessage(string input)
        {
            try
            {

                // Define a regular expression pattern to match the id value with or without backslashes
                var pattern = "(?<=\\\"id\\\":\\\")([^\"]+)(?=\\\")";

                // Create a regex object
                var regex = new Regex(pattern);

                // Find all matches in the input
                var matches = regex.Matches(input);

                // Extract and print the id values
                foreach (Match match in matches)
                {
                    return Guid.Parse(match.Value);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public virtual int GetUnreadMessageCount(UserIdentifier sender, UserIdentifier receiver)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(receiver.TenantId))
                {
                    return _chatMessageRepository.Count(cm => cm.UserId == receiver.UserId &&
                                                              cm.TargetUserId == sender.UserId &&
                                                              cm.TargetTenantId == sender.TenantId &&
                                                              cm.ReadState == ChatMessageReadState.Unread);
                }
            });
        }

        public async Task<ChatMessage> FindMessageAsync(int id, long userId)
        {
            return await _chatMessageRepository.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        private async Task HandleSenderToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message, Guid sharedMessageId)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier))?.State;
            if (friendshipState == null)
            {
                friendshipState = FriendshipState.Accepted;

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);

                var receiverUser = await _userManager.GetUserAsync(receiverIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        receiverTenancyName,
                        receiverUser.UserName,
                        receiverUser.ProfilePictureId,
                        friendshipState.Value)
                );
            }

            if (friendshipState.Value == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                return;
            }

            var sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Sender,
                message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread
            );

            Save(sentMessage);

            await _chatCommunicator.SendMessageToClient(
                _onlineClientManager.GetAllByUserId(senderIdentifier),
                sentMessage
                );
        }

        private async Task HandleReceiverToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message, Guid sharedMessageId)
        {
            var friendship = await _friendshipManager.GetFriendshipOrNullAsync(receiverIdentifier, senderIdentifier);
            var friendshipState = friendship?.State;
            var clients = _onlineClientManager.GetAllByUserId(receiverIdentifier);

            if (friendshipState == null)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);
                var senderUser = await _userManager.GetUserAsync(senderIdentifier);

                friendship = new Friendship(
                    receiverIdentifier,
                    senderIdentifier,
                    senderTenancyName,
                    senderUser.UserName,
                    senderUser.ProfilePictureId,
                    FriendshipState.Accepted
                );

                await _friendshipManager.CreateFriendshipAsync(friendship);

                if (clients.Any())
                {
                    var isFriendOnline = _onlineClientManager.IsOnline(receiverIdentifier);
                    await _chatCommunicator.SendFriendshipRequestToClient(clients, friendship, false, isFriendOnline);
                }
            }

            if (friendshipState == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                return;
            }

            var sentMessage = new ChatMessage(
                    receiverIdentifier,
                    senderIdentifier,
                    ChatSide.Receiver,
                    message,
                    ChatMessageReadState.Unread,
                    sharedMessageId,
                    ChatMessageReadState.Read
                );

            Save(sentMessage);

            if (clients.Any())
            {
                await _chatCommunicator.SendMessageToClient(clients, sentMessage);
            }
            else if (GetUnreadMessageCount(senderIdentifier, receiverIdentifier) == 1)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                await _userEmailer.TryToSendChatMessageMail(
                      await _userManager.GetUserAsync(receiverIdentifier),
                      (await _userManager.GetUserAsync(senderIdentifier)).UserName,
                      senderTenancyName,
                      sentMessage
                  );
            }
        }

        private async Task HandleSenderUserInfoChangeAsync(UserIdentifier sender, UserIdentifier receiver, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId)
        {
            var receiverCacheItem = _userFriendsCache.GetCacheItemOrNull(receiver);

            var senderAsFriend = receiverCacheItem?.Friends.FirstOrDefault(f => f.FriendTenantId == sender.TenantId && f.FriendUserId == sender.UserId);
            if (senderAsFriend == null)
            {
                return;
            }

            if (senderAsFriend.FriendTenancyName == senderTenancyName &&
                senderAsFriend.FriendUserName == senderUserName &&
                senderAsFriend.FriendProfilePictureId == senderProfilePictureId)
            {
                return;
            }

            var friendship = (await _friendshipManager.GetFriendshipOrNullAsync(receiver, sender));
            if (friendship == null)
            {
                return;
            }

            friendship.FriendTenancyName = senderTenancyName;
            friendship.FriendUserName = senderUserName;
            friendship.FriendProfilePictureId = senderProfilePictureId;

            await _friendshipManager.UpdateFriendshipAsync(friendship);
        }

        private async Task<string> GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _tenantCache.GetAsync(tenantId.Value);
                return tenant.TenancyName;
            }

            return null;
        }
    }
}

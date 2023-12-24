using System;
using System.Collections.Generic;
using Abp.Domain.Repositories;
using Chamran.Deed.Chat.Dto;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using Chamran.Deed.Friendships.Cache;
using Chamran.Deed.Friendships.Dto;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Chat;

namespace Chamran.Deed.Chat
{
    [AbpAuthorize]
    public class ChatAppService : DeedAppServiceBase, IChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IRepository<User, long> _userRepository;

        public ChatAppService(IRepository<ChatMessage, long> chatMessageRepository,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager<ChatChannel> onlineClientManager,
            IChatCommunicator chatCommunicator, IRepository<User, long> userRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _userRepository = userRepository;
        }

        [DisableAuditing]
        public async Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings()
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            if (userIdentifier == null)
            {
                return new GetUserChatFriendsWithSettingsOutput();
            }
            //var user = await UserManager.GetUserAsync(AbpSession.ToUserIdentifier());
            //if (user.IsSuperUser)
            //{
            //    return new GetUserChatFriendsWithSettingsOutput
            //    {
            //        Friends = friends,
            //        ServerTime = Clock.Now
            //    };
            //}
            //else
            {

                var cacheItem = _userFriendsCache.GetCacheItem(userIdentifier);
                var friends = ObjectMapper.Map<List<FriendDto>>(cacheItem.Friends);

                foreach (var friend in friends)
                {
                    friend.IsOnline = _onlineClientManager.IsOnline(
                        new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                    );
                    var query = await _chatMessageRepository.GetAll()
                        .Where(m => m.UserId == AbpSession.UserId && m.TargetTenantId == AbpSession.TenantId && m.TargetUserId == friend.FriendUserId)
                        .OrderByDescending(m => m.CreationTime)
                        .Take(1)
                        .ToListAsync();
                    if (!query.Any()) continue;
                    var entity = query.First();
                    friend.LatestMessage = entity.Message;
                    friend.LastMessageDateTime = entity.CreationTime;
                    //friend.FriendUserId = entity.;
                }

                return new GetUserChatFriendsWithSettingsOutput
                {
                    Friends = friends,
                    ServerTime = Clock.Now
                };
            }

        }


        [DisableAuditing]
        public async Task<GetPagedUserChatFriendsWithSettingsOutput> GetPagedUserChatFriendsWithSettings(GetUserChatFriendsWithSettingsInput input)
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            if (userIdentifier == null)
            {
                return new GetPagedUserChatFriendsWithSettingsOutput()
                {
                    Friends = new PagedResultDto<FriendDto>()
                    { Items = new List<FriendDto>(), TotalCount = 0 },
                    ServerTime = Clock.Now,
                };
            }

            var cacheItem = _userFriendsCache.GetCacheItem(userIdentifier);
            var friends = ObjectMapper.Map<List<FriendDto>>(cacheItem.Friends);

            foreach (var friend in friends)
            {
                friend.IsOnline = _onlineClientManager.IsOnline(
                    new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                );
                var query = await _chatMessageRepository.GetAll()
                    .Where(m => m.UserId == AbpSession.UserId && m.TargetTenantId == AbpSession.TenantId && m.TargetUserId == friend.FriendUserId)
                    .OrderByDescending(m => m.CreationTime)
                    .Take(1)
                 
                    .ToListAsync();
                if (query.Any())
                {
                    var entity = query.First();
                    
                    friend.LatestMessage = entity.Message;
                    friend.LastMessageDateTime = entity.CreationTime;
                
                }
                var userResult = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Id == friend.FriendUserId);
                if(userResult != null)
                {
                    friend.FriendName = userResult.Name;
                    friend.FriendSurName = userResult.Surname;
                    friend.FriendProfilePictureId = userResult.ProfilePictureId;
                }
            }
            var pagedAndFilteredFriends = friends.OrderByDescending(x => x.LastMessageDateTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new GetPagedUserChatFriendsWithSettingsOutput()
            {
                Friends = new PagedResultDto<FriendDto>(
                    friends.Count,
                    pagedAndFilteredFriends
                    ),
                ServerTime = Clock.Now,
            };
        }

        [DisableAuditing]
        public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input)
        {
            var userId = AbpSession.GetUserId();
            var query = from x in _chatMessageRepository.GetAll().WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                    .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                    .OrderByDescending(m => m.CreationTime)
                    .Take(50)
                        join r in _chatMessageRepository.GetAll() on x.ReplyMessageId equals r.Id into joiner
                        from r in joiner.DefaultIfEmpty()
                        select new ChatMessageDto
                        {
                            UserId = x.UserId,
                            CreationTime = x.CreationTime,
                            ReplyMessage = r.Message,
                            ReplyMessageId = x.ReplyMessageId,
                            ForwardedFromName = x.ForwardedFromName,
                            TenantId=x.TenantId,
                            TargetUserId=x.TargetUserId,
                            TargetTenantId=x.TargetTenantId,
                            Side = x.Side,
                            ReadState=x.ReadState,
                            ReceiverReadState=x.ReceiverReadState,
                            Message=x.Message,
                            SharedMessageId=x.SharedMessageId,
                            };
            
            var messages =await query.ToListAsync();
            messages.Reverse();
            foreach (var chatMessageDto in messages)
            {
                if (string.IsNullOrWhiteSpace(chatMessageDto.ForwardedFromName))
                    chatMessageDto.ForwardedFromName = null;
            }
            return new ListResultDto<ChatMessageDto>(messages);
        }

        public async Task<PagedResultDto<ChatMessageDto>> GetPagedUserChatMessages(GetPagedUserChatMessagesInput input)
        {
            var userId = AbpSession.GetUserId();
            var countQuery = _chatMessageRepository.GetAll()
                .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                .OrderByDescending(m => m.CreationTime)
                .Count();
            var messages = await _chatMessageRepository.GetAll()
                .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                .OrderByDescending(m => m.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            messages.Reverse();

            var res = ObjectMapper.Map<List<ChatMessageDto>>(messages);

            return new PagedResultDto<ChatMessageDto>(
                countQuery,
                res
            );
        }

        public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            var userId = AbpSession.GetUserId();
            var tenantId = AbpSession.TenantId;

            // receiver messages
            var messages = await _chatMessageRepository
                 .GetAll()
                 .Where(m =>
                        m.UserId == userId &&
                        m.TargetTenantId == input.TenantId &&
                        m.TargetUserId == input.UserId &&
                        m.ReadState == ChatMessageReadState.Unread || m.ReadState == ChatMessageReadState.Received)
                 .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
            }

            // sender messages
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var reverseMessages = await _chatMessageRepository.GetAll()
                    .Where(m => m.UserId == input.UserId && m.TargetTenantId == tenantId && m.TargetUserId == userId)
                    .ToListAsync();

                if (!reverseMessages.Any())
                {
                    return;
                }

                foreach (var message in reverseMessages)
                {
                    message.ChangeReceiverReadState(ChatMessageReadState.Read);
                }
            }

            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = input.ToUserIdentifier();

            _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

            var onlineUserClients = _onlineClientManager.GetAllByUserId(userIdentifier);
            if (onlineUserClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
            }

            var onlineFriendClients = _onlineClientManager.GetAllByUserId(friendIdentifier);
            if (onlineFriendClients.Any())
            {
                await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
            }
        }

        public async Task MarkAllUnreadMessagesOfUserAsReceived(MarkAllUnreadMessagesOfUserAsReceivedInput input)
        {
            var userId = AbpSession.GetUserId();
            var tenantId = AbpSession.TenantId;

            // receiver messages
            var messages = await _chatMessageRepository
                 .GetAll()
                 .Where(m =>
                        m.UserId == userId &&
                        m.TargetTenantId == input.TenantId &&
                        m.TargetUserId == input.UserId &&
                        m.ReadState == ChatMessageReadState.Unread)
                 .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Received);
            }

            // sender messages
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var reverseMessages = await _chatMessageRepository.GetAll()
                    .Where(m => m.UserId == input.UserId && m.TargetTenantId == tenantId && m.TargetUserId == userId)
                    .ToListAsync();

                if (!reverseMessages.Any())
                {
                    return;
                }

                foreach (var message in reverseMessages)
                {
                    message.ChangeReceiverReadState(ChatMessageReadState.Received);
                }
            }

            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = input.ToUserIdentifier();

            _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

            var onlineUserClients = _onlineClientManager.GetAllByUserId(userIdentifier);
            if (onlineUserClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
            }

            var onlineFriendClients = _onlineClientManager.GetAllByUserId(friendIdentifier);
            if (onlineFriendClients.Any())
            {
                await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
            }
        }

        public async Task MarkMessageAsReceived(MarkMessageAsReceivedInput input)
        {
            var userId = AbpSession.GetUserId();
            var tenantId = AbpSession.TenantId;

            // receiver messages
            var messages = await _chatMessageRepository
                 .GetAll()
                 .Where(m =>
                        m.UserId == userId &&
                        m.TargetTenantId == input.TenantId &&
                        m.TargetUserId == input.UserId &&
                        m.ReadState == ChatMessageReadState.Unread &&
                        m.SharedMessageId == input.SharedMessageId)
                 .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Received);
            }

            // sender messages
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var reverseMessages = await _chatMessageRepository.GetAll()
                    .Where(m => m.UserId == input.UserId && m.TargetTenantId == tenantId && m.TargetUserId == userId && m.SharedMessageId == input.SharedMessageId)
                    .ToListAsync();

                if (!reverseMessages.Any())
                {
                    return;
                }

                foreach (var message in reverseMessages)
                {
                    message.ChangeReceiverReadState(ChatMessageReadState.Received);
                }
            }

            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = input.ToUserIdentifier();

            _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

            var onlineUserClients = _onlineClientManager.GetAllByUserId(userIdentifier);
            if (onlineUserClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
            }

            var onlineFriendClients = _onlineClientManager.GetAllByUserId(friendIdentifier);
            if (onlineFriendClients.Any())
            {
                await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
            }
        }
    }
}
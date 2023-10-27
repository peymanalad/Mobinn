using Abp;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Chamran.Deed.Chat;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Chamran.Deed.Authorization.Users;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.Friendships.Cache
{
    public class UserFriendsCache : IUserFriendsCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly ITenantCache _tenantCache;
        private readonly UserStore _userStore;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        private readonly object _syncObj = new object();

        public UserFriendsCache(
            ICacheManager cacheManager,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            IRepository<User, long> userRepository,
            ITenantCache tenantCache,
            IUnitOfWorkManager unitOfWorkManager,
            UserStore userStore)
        {
            _cacheManager = cacheManager;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _userRepository = userRepository;
            _tenantCache = tenantCache;
            _unitOfWorkManager = unitOfWorkManager;
            _userStore = userStore;
        }

        public virtual UserWithFriendsCacheItem GetCacheItem(UserIdentifier userIdentifier)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                return _cacheManager
                    .GetCache(FriendCacheItem.CacheName)
                    .AsTyped<string, UserWithFriendsCacheItem>()
                    .Get(userIdentifier.ToUserIdentifierString(), f => GetUserFriendsCacheItemInternal(userIdentifier));
            });
        }

        public virtual UserWithFriendsCacheItem GetCacheItemOrNull(UserIdentifier userIdentifier)
        {
            return _cacheManager
                .GetCache(FriendCacheItem.CacheName)
                .AsTyped<string, UserWithFriendsCacheItem>()
                .GetOrDefault(userIdentifier.ToUserIdentifierString());
        }

        public virtual void ResetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier friendIdentifier)
        {
            _unitOfWorkManager.WithUnitOfWork(() =>
            {
                var user = GetCacheItemOrNull(userIdentifier);
                if (user == null)
                {
                    return;
                }

                lock (_syncObj)
                {
                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId &&
                             f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    friend.UnreadMessageCount = 0;
                    UpdateUserOnCache(userIdentifier, user);
                }
            });
        }

        public virtual void IncreaseUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier friendIdentifier,
            int change)
        {
            _unitOfWorkManager.WithUnitOfWork(() =>
            {
                var user = GetCacheItemOrNull(userIdentifier);
                if (user == null)
                {
                    return;
                }

                lock (_syncObj)
                {
                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId &&
                             f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    friend.UnreadMessageCount += change;
                    UpdateUserOnCache(userIdentifier, user);
                }
            });
        }

        public void AddFriend(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = GetCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (!user.Friends.ContainsFriend(friend))
                {
                    user.Friends.Add(friend);
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        public void RemoveFriend(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = GetCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (user.Friends.ContainsFriend(friend))
                {
                    user.Friends.RemoveCachedFriend(friend);
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        public void UpdateFriend(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            var user = GetCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                var existingFriendIndex = user.Friends.FindIndex(
                    f => f.FriendUserId == friend.FriendUserId &&
                         f.FriendTenantId == friend.FriendTenantId
                );

                if (existingFriendIndex >= 0)
                {
                    user.Friends[existingFriendIndex] = friend;
                    UpdateUserOnCache(userIdentifier, user);
                }
            }
        }

        protected virtual UserWithFriendsCacheItem GetUserFriendsCacheItemInternal(UserIdentifier userIdentifier)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                var tenancyName = userIdentifier.TenantId.HasValue
                    ? _tenantCache.GetOrNull(userIdentifier.TenantId.Value)?.TenancyName
                    : null;

                using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
                {
                    var friendCacheItems = _friendshipRepository.GetAll()
                        .Where(friendship => friendship.UserId == userIdentifier.UserId)
                        .Join(_userRepository.GetAll(),
                            friendship => friendship.FriendUserId,
                            user => user.Id,
                            (friendship, user) => new
                            {
                                Friendship = friendship,
                                User = user
                            })
                        .Select(result => new FriendCacheItem
                        {
                            FriendUserId = result.Friendship.FriendUserId,
                            FriendTenantId = result.Friendship.FriendTenantId,
                            State = result.Friendship.State,
                            FriendUserName = result.Friendship.FriendUserName,
                            FriendName = result.User.Name,
                            FriendSurName = result.User.Surname,
                            FriendTenancyName = result.Friendship.FriendTenancyName,
                            FriendProfilePictureId = result.User.ProfilePictureId,
                            UnreadMessageCount = _chatMessageRepository.GetAll()
                                .Count(cm => cm.ReadState == ChatMessageReadState.Unread &&
                                             cm.UserId == userIdentifier.UserId &&
                                             cm.TenantId == userIdentifier.TenantId &&
                                             cm.TargetUserId == result.Friendship.FriendUserId &&
                                             cm.TargetTenantId == result.Friendship.FriendTenantId &&
                                             cm.Side == ChatSide.Receiver)
                        })
                        .AsEnumerable()
                        .OrderByDescending(x => x.LastMessageDateTime)
                        .ToList();

                    var user = _userStore.FindById(userIdentifier.UserId.ToString());

                    return new UserWithFriendsCacheItem
                    {
                        TenantId = userIdentifier.TenantId,
                        UserId = userIdentifier.UserId,
                        TenancyName = tenancyName,
                        UserName = user.UserName,
                        ProfilePictureId = user.ProfilePictureId,
                        Friends = friendCacheItems
                    };
                }
            });
        }

        private void UpdateUserOnCache(UserIdentifier userIdentifier, UserWithFriendsCacheItem user)
        {
            _cacheManager.GetCache(FriendCacheItem.CacheName).Set(userIdentifier.ToUserIdentifierString(), user);
        }
    }
}

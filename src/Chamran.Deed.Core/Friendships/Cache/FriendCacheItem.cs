using System;
using Abp.AutoMapper;

namespace Chamran.Deed.Friendships.Cache
{
    [AutoMapFrom(typeof(Friendship))]
    public class FriendCacheItem
    {
        public const string CacheName = "AppUserFriendCache";

        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }

        public string FriendUserName { get; set; }

        public string FriendTenancyName { get; set; }

        public Guid? FriendProfilePictureId { get; set; }

        public int UnreadMessageCount { get; set; }

        public FriendshipState State { get; set; }

        public string LatestMessage { get; set; }

        public DateTime LastMessageDateTime { get; set; }

        public string FriendName { get; set; }

        public string FriendSurName { get; set; }
    }
}
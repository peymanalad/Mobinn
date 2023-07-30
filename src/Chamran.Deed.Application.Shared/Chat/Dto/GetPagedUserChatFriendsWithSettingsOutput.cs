using System;
using Abp.Application.Services.Dto;
using Chamran.Deed.Friendships.Dto;

namespace Chamran.Deed.Chat.Dto
{
    public class GetPagedUserChatFriendsWithSettingsOutput
    {
        public PagedResultDto<FriendDto> Friends { get; set; }
        public DateTime ServerTime { get; set; }


        public GetPagedUserChatFriendsWithSettingsOutput()
        {
            Friends = new PagedResultDto<FriendDto>();
        }
    }
}
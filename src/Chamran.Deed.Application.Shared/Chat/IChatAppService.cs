using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Chat.Dto;

namespace Chamran.Deed.Chat
{
    public interface IChatAppService : IApplicationService
    {
        Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings();

        Task<GetPagedUserChatFriendsWithSettingsOutput> GetPagedUserChatFriendsWithSettings(GetUserChatFriendsWithSettingsInput input);

        Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input);

        Task<PagedResultDto<ChatMessageDto>> GetPagedUserChatMessages(GetPagedUserChatMessagesInput input);

        Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input);
        Task MarkAllUnreadMessagesOfUserAsReceived(MarkAllUnreadMessagesOfUserAsReceivedInput input);
    }
}

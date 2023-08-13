using System;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;

namespace Chamran.Deed.Chat
{
    public interface IChatMessageManager : IDomainService
    {
        Task DeleteMessageAsync(UserIdentifier sender, Guid sharedMessageId);

        Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId);

        Task EditMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid sharedMessageId,string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId);

        long Save(ChatMessage message);

        int GetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier sender);

        Task<ChatMessage> FindMessageAsync(int id, long userId);
    }
}

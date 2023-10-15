using Abp;

namespace Chamran.Deed.Chat.Dto
{
    public class MarkAllUnreadMessagesOfUserAsReceivedInput
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, UserId);
        }
    }
}
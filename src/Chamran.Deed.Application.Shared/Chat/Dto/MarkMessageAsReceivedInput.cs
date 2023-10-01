using System;
using Abp;

namespace Chamran.Deed.Chat.Dto
{
    public class MarkMessageAsReceivedInput
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }
        public Guid SharedMessageId{ get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, UserId);
        }
    }
}
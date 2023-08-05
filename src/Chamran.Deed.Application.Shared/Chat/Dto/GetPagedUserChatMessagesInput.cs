using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Chat.Dto
{
    public class GetPagedUserChatMessagesInput: PagedAndSortedResultRequestDto
    {
        public int? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        public long? MinMessageId { get; set; }
    }
}
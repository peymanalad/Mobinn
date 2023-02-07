using Chamran.Deed.Dto;

namespace Chamran.Deed.WebHooks.Dto
{
    public class GetAllSendAttemptsInput : PagedInputDto
    {
        public string SubscriptionId { get; set; }
    }
}

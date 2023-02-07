using System.Threading.Tasks;
using Abp.Webhooks;

namespace Chamran.Deed.WebHooks
{
    public interface IWebhookEventAppService
    {
        Task<WebhookEvent> Get(string id);
    }
}

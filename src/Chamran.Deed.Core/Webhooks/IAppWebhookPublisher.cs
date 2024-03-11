using System.Threading.Tasks;

namespace Chamran.Deed.Webhooks
{
    public interface IAppWebhookPublisher
    {
        Task PublishTestWebhook();
    }
}

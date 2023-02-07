using System.Threading.Tasks;
using Chamran.Deed.Authorization.Users;

namespace Chamran.Deed.WebHooks
{
    public interface IAppWebhookPublisher
    {
        Task PublishTestWebhook();
    }
}

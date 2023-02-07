using System.Threading.Tasks;
using Abp.Application.Services;

namespace Chamran.Deed.MultiTenancy
{
    public interface ISubscriptionAppService : IApplicationService
    {
        Task DisableRecurringPayments();

        Task EnableRecurringPayments();
    }
}

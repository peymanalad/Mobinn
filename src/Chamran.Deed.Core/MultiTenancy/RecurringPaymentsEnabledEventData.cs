using Abp.Events.Bus;

namespace Chamran.Deed.MultiTenancy
{
    public class RecurringPaymentsEnabledEventData : EventData
    {
        public int TenantId { get; set; }
    }
}
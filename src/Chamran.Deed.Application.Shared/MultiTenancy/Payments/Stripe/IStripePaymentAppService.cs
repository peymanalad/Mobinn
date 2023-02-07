using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.MultiTenancy.Payments.Dto;
using Chamran.Deed.MultiTenancy.Payments.Stripe.Dto;

namespace Chamran.Deed.MultiTenancy.Payments.Stripe
{
    public interface IStripePaymentAppService : IApplicationService
    {
        Task ConfirmPayment(StripeConfirmPaymentInput input);

        StripeConfigurationDto GetConfiguration();

        Task<SubscriptionPaymentDto> GetPaymentAsync(StripeGetPaymentInput input);

        Task<string> CreatePaymentSession(StripeCreatePaymentSessionInput input);
    }
}
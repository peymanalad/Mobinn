using Chamran.Deed.Editions.Dto;

namespace Chamran.Deed.MultiTenancy.Payments.Dto
{
    public class PaymentInfoDto
    {
        public EditionSelectDto Edition { get; set; }

        public decimal AdditionalPrice { get; set; }

        public bool IsLessThanMinimumUpgradePaymentAmount()
        {
            return AdditionalPrice < DeedConsts.MinimumUpgradePaymentAmount;
        }
    }
}

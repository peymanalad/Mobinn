﻿using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.MultiTenancy.Payments.PayPal.Dto;

namespace Chamran.Deed.MultiTenancy.Payments.PayPal
{
    public interface IPayPalPaymentAppService : IApplicationService
    {
        Task ConfirmPayment(long paymentId, string paypalOrderId);

        PayPalConfigurationDto GetConfiguration();
    }
}

using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Chamran.Deed.MultiTenancy.Accounting.Dto;

namespace Chamran.Deed.MultiTenancy.Accounting
{
    public interface IInvoiceAppService
    {
        Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

        Task CreateInvoice(CreateInvoiceDto input);
    }
}

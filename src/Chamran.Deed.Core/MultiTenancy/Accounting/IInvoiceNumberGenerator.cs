using System.Threading.Tasks;
using Abp.Dependency;

namespace Chamran.Deed.MultiTenancy.Accounting
{
    public interface IInvoiceNumberGenerator : ITransientDependency
    {
        Task<string> GetNewInvoiceNumber();
    }
}
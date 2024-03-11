using System.Threading.Tasks;
using Abp.Domain.Uow;

namespace Chamran.Deed.OpenIddict
{
    public interface IOpenIddictDbConcurrencyExceptionHandler
    {
        Task HandleAsync(AbpDbConcurrencyException exception);
    }
}
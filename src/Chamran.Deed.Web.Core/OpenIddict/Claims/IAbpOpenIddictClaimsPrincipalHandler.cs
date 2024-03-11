using System.Threading.Tasks;

namespace Chamran.Deed.Web.OpenIddict.Claims
{
    public interface IAbpOpenIddictClaimsPrincipalHandler
    {
        Task HandleAsync(AbpOpenIddictClaimsPrincipalHandlerContext context);
    }
}
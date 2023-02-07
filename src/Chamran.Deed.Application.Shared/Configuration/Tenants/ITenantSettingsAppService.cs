using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.Configuration.Tenants.Dto;

namespace Chamran.Deed.Configuration.Tenants
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(TenantSettingsEditDto input);

        Task ClearLogo();

        Task ClearCustomCss();
    }
}

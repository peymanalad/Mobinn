using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.Configuration.Host.Dto;

namespace Chamran.Deed.Configuration.Host
{
    public interface IHostSettingsAppService : IApplicationService
    {
        Task<HostSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(HostSettingsEditDto input);

        Task SendTestEmail(SendTestEmailInput input);
    }
}

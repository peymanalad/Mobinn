using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.Install.Dto;

namespace Chamran.Deed.Install
{
    public interface IInstallAppService : IApplicationService
    {
        Task Setup(InstallDto input);

        AppSettingsJsonDto GetAppSettingsJson();

        CheckDatabaseOutput CheckDatabase();
    }
}
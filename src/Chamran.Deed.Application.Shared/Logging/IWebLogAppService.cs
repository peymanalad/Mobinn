using Abp.Application.Services;
using Chamran.Deed.Dto;
using Chamran.Deed.Logging.Dto;

namespace Chamran.Deed.Logging
{
    public interface IWebLogAppService : IApplicationService
    {
        GetLatestWebLogsOutput GetLatestWebLogs();

        FileDto DownloadWebLogs();
    }
}

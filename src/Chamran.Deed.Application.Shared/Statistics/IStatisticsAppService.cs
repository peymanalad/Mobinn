using System.Threading.Tasks;
using Chamran.Deed.Statistics.Dto;
using Abp.Application.Services;

namespace Chamran.Deed.Statistics
{
    public interface IStatisticsAppService : IApplicationService
    {
        Task<LoginStatsDto> GetLoginStats(GetLoginStatsInput input);

        Task<StatisticResultDto> GetActiveUsers(GetLoginStatsInput input);
        Task<StatisticResultDto> GetNewUsers(GetLoginStatsInput input);
        Task<StatisticResultDto> GetUserRoleDistribution(GetLoginStatsInput input);
        Task<StatisticResultDto> GetServiceCallCounts(GetLoginStatsInput input);
        Task<StatisticResultDto> GetServiceLatency(GetLoginStatsInput input);
        Task<StatisticResultDto> GetServiceErrorRates(GetLoginStatsInput input);
        Task<StatisticResultDto> GetUsageHeatmap(GetLoginStatsInput input);
        Task<StatisticResultDto> GetOrganizationGrowth(GetLoginStatsInput input);
        Task<StatisticResultDto> GetPostsStats(GetLoginStatsInput input);
        Task<StatisticResultDto> GetPostMediaDistribution(GetLoginStatsInput input);
        Task<StatisticResultDto> GetTopOrganizations(GetLoginStatsInput input);
        Task<StatisticResultDto> GetNotificationStats(GetLoginStatsInput input);
        Task<StatisticResultDto> GetOtpStats(GetLoginStatsInput input);
        Task<StatisticResultDto> GetSlowQueries(GetLoginStatsInput input);
        Task<StatisticResultDto> GetClientDistribution(GetLoginStatsInput input);
    }
}
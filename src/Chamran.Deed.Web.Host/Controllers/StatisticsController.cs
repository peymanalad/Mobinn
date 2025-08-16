using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Chamran.Deed.Statistics;
using Chamran.Deed.Statistics.Dto;

namespace Chamran.Deed.Web.Controllers
{
    [Authorize]
    [Route("api/stats")]
    public class StatisticsController : DeedControllerBase
    {
        private readonly IStatisticsAppService _statisticsAppService;

        public StatisticsController(IStatisticsAppService statisticsAppService)
        {
            _statisticsAppService = statisticsAppService;
        }

        [HttpGet("logins")]
        public Task<LoginStatsDto> GetLoginStats([FromQuery] GetLoginStatsInput input)
        {
            return _statisticsAppService.GetLoginStats(input);
        }

        // Additional metrics endpoints
        [HttpGet("users/active")]
        public Task<StatisticResultDto> GetActiveUsers([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetActiveUsers(input);

        [HttpGet("users/new")]
        public Task<StatisticResultDto> GetNewUsers([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetNewUsers(input);

        [HttpGet("users/churn-risk")]
        public Task<StatisticResultDto> GetChurnRisk([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetChurnRisk(input);

        [HttpGet("users/by-role")]
        public Task<StatisticResultDto> GetUserRoleDistribution([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetUserRoleDistribution(input);

        [HttpGet("services/calls")]
        public Task<StatisticResultDto> GetServiceCallCounts([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetServiceCallCounts(input);

        [HttpGet("services/latency")]
        public Task<StatisticResultDto> GetServiceLatency([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetServiceLatency(input);

        [HttpGet("services/errors")]
        public Task<StatisticResultDto> GetServiceErrorRates([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetServiceErrorRates(input);

        [HttpGet("search")]
        public Task<StatisticResultDto> GetSearchStats([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetSearchStats(input);

        [HttpGet("usage/heatmap")]
        public Task<StatisticResultDto> GetUsageHeatmap([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetUsageHeatmap(input);

        [HttpGet("orgs/growth")]
        public Task<StatisticResultDto> GetOrganizationGrowth([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetOrganizationGrowth(input);

        [HttpGet("posts")]
        public Task<StatisticResultDto> GetPostsStats([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetPostsStats(input);

        [HttpGet("posts/media-type")]
        public Task<StatisticResultDto> GetPostMediaDistribution([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetPostMediaDistribution(input);

        [HttpGet("orgs/top")]
        public Task<StatisticResultDto> GetTopOrganizations([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetTopOrganizations(input);

        [HttpGet("notifications")]
        public Task<StatisticResultDto> GetNotificationStats([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetNotificationStats(input);

        [HttpGet("otp")]
        public Task<StatisticResultDto> GetOtpStats([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetOtpStats(input);

        [HttpGet("db/slow-queries")]
        public Task<StatisticResultDto> GetSlowQueries([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetSlowQueries(input);

        [HttpGet("queues")]
        public Task<StatisticResultDto> GetQueueStatus([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetQueueStatus(input);

        [HttpGet("storage")]
        public Task<StatisticResultDto> GetStorageCapacity([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetStorageCapacity(input);

        [HttpGet("clients/devices")]
        public Task<StatisticResultDto> GetClientDistribution([FromQuery] GetLoginStatsInput input)
            => _statisticsAppService.GetClientDistribution(input);
    }
}
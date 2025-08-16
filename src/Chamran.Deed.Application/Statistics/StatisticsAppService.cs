using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Microsoft.EntityFrameworkCore;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.People;
using Chamran.Deed.Info;
using Chamran.Deed.Common;
using Chamran.Deed.Storage;
using Chamran.Deed.Statistics.Dto;

namespace Chamran.Deed.Statistics
{
    [AbpAuthorize]
    public class StatisticsAppService : DeedAppServiceBase, IStatisticsAppService
    {
        private readonly IRepository<UserLoginAttempt, long> _loginAttemptRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role, int> _roleRepository;
        private readonly IRepository<AuditLog, long> _auditLogRepository;
        private readonly IRepository<Organization> _organizationRepository;
        private readonly IRepository<Post> _postRepository;
        private readonly IRepository<PostLike> _postLikeRepository;
        private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;
        private readonly IRepository<FCMQueue, int> _fcmQueueRepository;
        private readonly IRepository<BinaryObject, Guid> _binaryObjectRepository;

        public StatisticsAppService(
            IRepository<UserLoginAttempt, long> loginAttemptRepository,
            IRepository<User, long> userRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role, int> roleRepository,
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<Organization> organizationRepository,
            IRepository<Post> postRepository,
            IRepository<PostLike> postLikeRepository,
            IRepository<UserNotificationInfo, Guid> userNotificationRepository,
            IRepository<FCMQueue, int> fcmQueueRepository,
            IRepository<BinaryObject, Guid> binaryObjectRepository)
        {
            _loginAttemptRepository = loginAttemptRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _auditLogRepository = auditLogRepository;
            _organizationRepository = organizationRepository;
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
            _userNotificationRepository = userNotificationRepository;
            _fcmQueueRepository = fcmQueueRepository;
            _binaryObjectRepository = binaryObjectRepository;
        }

        public async Task<LoginStatsDto> GetLoginStats(GetLoginStatsInput input)
        {
            var baseQuery = _loginAttemptRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.CreationTime <= input.EndDate.Value);

            var data = await baseQuery
                .Select(l => new { l.CreationTime, l.Result })
                .ToListAsync();

            var total = data.Count;
            var successful = data.Count(l => l.Result == AbpLoginResultType.Success);
            var failed = total - successful;
            var rate = total == 0 ? 0 : (double)successful / total;

            var groups = data
                .GroupBy(l => Truncate(l.CreationTime, input.Interval))
                .Select(g => new LoginStatsItemDto
                {
                    PeriodStart = g.Key,
                    Total = g.Count(),
                    Successful = g.Count(x => x.Result == AbpLoginResultType.Success)
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            foreach (var item in groups)
            {
                item.Failed = item.Total - item.Successful;
                item.SuccessRate = item.Total == 0 ? 0 : (double)item.Successful / item.Total;
            }

            return new LoginStatsDto
            {
                Total = total,
                Successful = successful,
                Failed = failed,
                SuccessRate = rate,
                Items = groups
            };
        }

        private static DateTime Truncate(DateTime date, StatisticsInterval interval)
        {
            switch (interval)
            {
                case StatisticsInterval.Weekly:
                    return date.Date.AddDays(-(int)date.DayOfWeek);
                case StatisticsInterval.Monthly:
                    return new DateTime(date.Year, date.Month, 1);
                default:
                    return date.Date;
            }
        }

        private static double CalculatePercentile(IList<int> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
            {
                return 0;
            }

            var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
            if (index < 0)
            {
                index = 0;
            }

            if (index >= sortedValues.Count)
            {
                index = sortedValues.Count - 1;
            }

            return sortedValues[index];
        }

        public async Task<StatisticResultDto> GetActiveUsers(GetLoginStatsInput input)
        {
            var baseQuery = _loginAttemptRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.CreationTime <= input.EndDate.Value)
                .Where(l => l.UserId.HasValue && l.Result == AbpLoginResultType.Success);

            var data = await baseQuery
                .Select(l => new { l.CreationTime, l.UserId })
                .ToListAsync();

            var groups = data
                .GroupBy(l => Truncate(l.CreationTime, input.Interval))
                .Select(g => new CountPerPeriodDto
                {
                    PeriodStart = g.Key,
                    Count = g.Select(x => x.UserId).Distinct().Count()
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            var total = groups.Sum(g => g.Count);

            return new StatisticResultDto
            {
                Metric = "ActiveUsers",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "items", groups }
                }
            };
        }

        public async Task<StatisticResultDto> GetNewUsers(GetLoginStatsInput input)
        {
            var baseQuery = _userRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, u => u.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, u => u.CreationTime <= input.EndDate.Value)
                .Select(u => u.CreationTime);

            var data = await baseQuery.ToListAsync();

            var groups = data
                .GroupBy(d => Truncate(d, input.Interval))
                .Select(g => new CountPerPeriodDto
                {
                    PeriodStart = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            var total = groups.Sum(g => g.Count);

            return new StatisticResultDto
            {
                Metric = "NewUsers",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "items", groups }
                }
            };
        }

        public async Task<StatisticResultDto> GetChurnRisk(GetLoginStatsInput input)
        {
            var threshold = input.StartDate ?? DateTime.UtcNow.AddDays(-30);

            var activeUserIds = await _loginAttemptRepository.GetAll()
                .Where(l => l.CreationTime >= threshold)
                .Where(l => l.Result == AbpLoginResultType.Success)
                .Where(l => l.UserId.HasValue)
                .Select(l => l.UserId.Value)
                .Distinct()
                .ToListAsync();

            var inactiveUsers = await _userRepository.GetAll()
                .Where(u => !activeUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return new StatisticResultDto
            {
                Metric = "ChurnRisk",
                Data = new Dictionary<string, object>
                {
                    { "count", inactiveUsers.Count },
                    { "users", inactiveUsers }
                }
            };
        }

        public async Task<StatisticResultDto> GetUserRoleDistribution(GetLoginStatsInput input)
        {
            var query = from ur in _userRoleRepository.GetAll()
                        join r in _roleRepository.GetAll() on ur.RoleId equals r.Id
                        group ur by r.DisplayName into g
                        select new KeyValueStatDto { Key = g.Key, Value = g.Count() };

            var items = await query.ToListAsync();

            return new StatisticResultDto
            {
                Metric = "UserRoleDistribution",
                Data = new Dictionary<string, object>
                {
                    { "items", items }
                }
            };
        }

        public async Task<StatisticResultDto> GetServiceCallCounts(GetLoginStatsInput input)
        {
            var query = _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value);

            var items = await query
                .GroupBy(l => new { l.ServiceName, l.MethodName })
                .Select(g => new ServiceCallCountDto
                {
                    ServiceName = g.Key.ServiceName,
                    MethodName = g.Key.MethodName,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(input.Top)
                .ToListAsync();

            return new StatisticResultDto
            {
                Metric = "ServiceCallCounts",
                Data = new Dictionary<string, object>
                {
                    { "items", items }
                }
            };
        }

        public async Task<StatisticResultDto> GetServiceLatency(GetLoginStatsInput input)
        {
            var data = await _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Select(l => new { l.ServiceName, l.MethodName, l.ExecutionDuration })
                .ToListAsync();

            var items = data
                .GroupBy(l => new { l.ServiceName, l.MethodName })
                .Select(g =>
                {
                    var durations = g.Select(x => x.ExecutionDuration).OrderBy(x => x).ToList();
                    return new ServiceLatencyDto
                    {
                        ServiceName = g.Key.ServiceName,
                        MethodName = g.Key.MethodName,
                        Average = durations.Any() ? durations.Average() : 0,
                        P95 = CalculatePercentile(durations, 0.95),
                        P99 = CalculatePercentile(durations, 0.99)
                    };
                })
                .OrderByDescending(x => x.P95)
                .Take(input.Top)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "ServiceLatency",
                Data = new Dictionary<string, object>
                {
                    { "items", items }
                }
            };
        }

        public async Task<StatisticResultDto> GetServiceErrorRates(GetLoginStatsInput input)
        {
            var data = await _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Where(l => l.ServiceName != null && l.MethodName != null)
                .Select(l => new
                {
                    l.ServiceName,
                    l.MethodName,
                    l.Exception
                })
                .ToListAsync();

            var map = new Dictionary<(string Service, string Method), ServiceErrorRateDto>();
            var exMap = new Dictionary<(string Service, string Method), Dictionary<string, int>>();

            foreach (var log in data)
            {
                var key = (log.ServiceName, log.MethodName);

                if (!map.TryGetValue(key, out var dto))
                {
                    dto = new ServiceErrorRateDto
                    {
                        ServiceName = log.ServiceName,
                        MethodName = log.MethodName,
                        Error4xx = 0,
                        Error5xx = 0
                    };
                    map[key] = dto;
                    exMap[key] = new Dictionary<string, int>(StringComparer.Ordinal);
                }

                dto.Total++;

                var hasError = !string.IsNullOrEmpty(log.Exception);
                if (hasError)
                {
                    dto.Fail++;
                    var dict = exMap[key];
                    dict[log.Exception] = dict.TryGetValue(log.Exception, out var c) ? c + 1 : 1;
                }
                else
                {
                    dto.Success++;
                }
            }

            foreach (var pair in map)
            {
                var dto = pair.Value;
                dto.Rate4xx = 0;
                dto.Rate5xx = 0;

                dto.TopException = exMap[pair.Key]
                    .OrderByDescending(x => x.Value)
                    .Select(x => x.Key)
                    .FirstOrDefault();
            }

            var take = input.Top > 0 ? input.Top : 10;

            var items = map.Values
                .OrderByDescending(x => x.Fail) 
                .ThenBy(x => x.ServiceName)
                .ThenBy(x => x.MethodName)
                .Take(take)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "ServiceErrorRates",
                Data = new Dictionary<string, object>
        {
            { "items", items }
        }
            };
        }


        public async Task<StatisticResultDto> GetSearchStats(GetLoginStatsInput input)
        {
            var baseQuery = _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Where(l => l.ServiceName != null && l.ServiceName.Contains("Search"));

            var data = await baseQuery
                .Select(l => new { l.ExecutionTime, l.ReturnValue })
                .ToListAsync();

            var total = data.Count;
            var noResult = data.Count(x => string.IsNullOrEmpty(x.ReturnValue) || x.ReturnValue == "[]" || x.ReturnValue.Contains("\"totalCount\":0"));

            var groups = data
                .GroupBy(x => Truncate(x.ExecutionTime, input.Interval))
                .Select(g => new CountPerPeriodDto
                {
                    PeriodStart = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "SearchStats",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "noResult", noResult },
                    { "noResultRate", total == 0 ? 0 : (double)noResult / total },
                    { "items", groups }
                }
            };
        }

        public async Task<StatisticResultDto> GetUsageHeatmap(GetLoginStatsInput input)
        {
            var timestamps = await _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Select(l => l.ExecutionTime)
                .ToListAsync();

            var items = timestamps
                .GroupBy(t => new { t.DayOfWeek, t.Hour })
                .Select(g => new HeatmapPointDto
                {
                    DayOfWeek = (int)g.Key.DayOfWeek,
                    Hour = g.Key.Hour,
                    Count = g.Count()
                })
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.Hour)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "UsageHeatmap",
                Data = new Dictionary<string, object>
                {
                    { "items", items }
                }
            };
        }

        public async Task<StatisticResultDto> GetOrganizationGrowth(GetLoginStatsInput input)
        {
            var creationTimes = await _organizationRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, o => o.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, o => o.CreationTime <= input.EndDate.Value)
                .Select(o => o.CreationTime)
                .ToListAsync();

            var groups = creationTimes
                .GroupBy(d => Truncate(d, input.Interval))
                .Select(g => new CountPerPeriodDto
                {
                    PeriodStart = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            var total = groups.Sum(g => g.Count);

            return new StatisticResultDto
            {
                Metric = "OrganizationGrowth",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "items", groups }
                }
            };
        }

        public async Task<StatisticResultDto> GetPostsStats(GetLoginStatsInput input)
        {
            var posts = await _postRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, p => p.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.CreationTime <= input.EndDate.Value)
                .Select(p => new
                {
                    p.CreationTime,
                    OrgId = p.PostGroupFk.OrganizationId,
                    OrgName = p.PostGroupFk.OrganizationFk.OrganizationName
                })
                .ToListAsync();

            var total = posts.Count;

            var perPeriod = posts
                .GroupBy(p => Truncate(p.CreationTime, input.Interval))
                .Select(g => new CountPerPeriodDto
                {
                    PeriodStart = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.PeriodStart)
                .ToList();

            var perOrg = posts
                .Where(p => p.OrgId.HasValue)
                .GroupBy(p => new { p.OrgId, p.OrgName })
                .Select(g => new KeyValueStatDto { Key = g.Key.OrgName, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "PostsStats",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "items", perPeriod },
                    { "byOrg", perOrg }
                }
            };
        }

        public async Task<StatisticResultDto> GetPostMediaDistribution(GetLoginStatsInput input)
        {
            var data = await _postRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, p => p.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.CreationTime <= input.EndDate.Value)
                .Select(p => new
                {
                    Type = p.PdfFile != null ? "PDF" :
                           (p.PostVideoPreview != null ? "Video" :
                           (p.PostFile != null || p.PostFile2 != null || p.PostFile3 != null || p.PostFile4 != null || p.PostFile5 != null || p.PostFile6 != null || p.PostFile7 != null || p.PostFile8 != null || p.PostFile9 != null || p.PostFile10 != null) ? "Image" : "Other")
                })
                .ToListAsync();

            var total = data.Count;

            var items = data
                .GroupBy(x => x.Type)
                .Select(g => new KeyValueStatDto { Key = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            var rates = items
                .Select(i => new { key = i.Key, rate = total == 0 ? 0 : (double)i.Value / total })
                .ToList();

            return new StatisticResultDto
            {
                Metric = "PostMediaDistribution",
                Data = new Dictionary<string, object>
                {
                    { "total", total },
                    { "items", items },
                    { "rates", rates }
                }
            };
        }

        public async Task<StatisticResultDto> GetTopOrganizations(GetLoginStatsInput input)
        {
            var postCounts = await _postRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, p => p.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.CreationTime <= input.EndDate.Value)
                .Where(p => p.PostGroupFk.OrganizationId.HasValue)
                .GroupBy(p => new { p.PostGroupFk.OrganizationId, p.PostGroupFk.OrganizationFk.OrganizationName })
                .Select(g => new KeyValueStatDto { Key = g.Key.OrganizationName, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(input.Top)
                .ToListAsync();

            var likeData = await (from l in _postLikeRepository.GetAll()
                                  join p in _postRepository.GetAll() on l.PostId equals p.Id
                                  where (!input.StartDate.HasValue || l.LikeTime >= input.StartDate.Value)
                                     && (!input.EndDate.HasValue || l.LikeTime <= input.EndDate.Value)
                                     && p.PostGroupFk.OrganizationId.HasValue
                                  select new { p.PostGroupFk.OrganizationId, p.PostGroupFk.OrganizationFk.OrganizationName })
                                  .ToListAsync();

            var likeCounts = likeData
                .GroupBy(x => new { x.OrganizationId, x.OrganizationName })
                .Select(g => new KeyValueStatDto { Key = g.Key.OrganizationName, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(input.Top)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "TopOrganizations",
                Data = new Dictionary<string, object>
                {
                    { "topByPosts", postCounts },
                    { "topByLikes", likeCounts }
                }
            };
        }

        public async Task<StatisticResultDto> GetNotificationStats(GetLoginStatsInput input)
        {
            var fcmQuery = _fcmQueueRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, q => q.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, q => q.CreationTime <= input.EndDate.Value);

            var sent = await fcmQuery.CountAsync();
            var delivered = await fcmQuery.CountAsync(q => q.IsSent);

            var opened = await _userNotificationRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, n => n.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, n => n.CreationTime <= input.EndDate.Value)
                .CountAsync(n => n.State == UserNotificationState.Read);

            return new StatisticResultDto
            {
                Metric = "NotificationStats",
                Data = new Dictionary<string, object>
                {
                    { "sent", sent },
                    { "delivered", delivered },
                    { "opened", opened },
                    { "deliveryRate", sent == 0 ? 0 : (double)delivered / sent },
                    { "openRate", sent == 0 ? 0 : (double)opened / sent }
                }
            };
        }

        public async Task<StatisticResultDto> GetOtpStats(GetLoginStatsInput input)
        {
            var baseQuery = _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Where(l => l.ServiceName != null && l.ServiceName.Contains("TokenAuth"));

            var sent = await baseQuery
                .Where(l => l.MethodName == "SendOtp")
                .CountAsync();

            //var authLogs = await baseQuery
            //    .Where(l => l.MethodName == "OtpAuthenticate")
            //    .Select(l => new { l.Exception, l.HttpStatusCode })
            //    .ToListAsync();

            //var success = authLogs.Count(l => string.IsNullOrEmpty(l.Exception) && l.HttpStatusCode >= 200 && l.HttpStatusCode < 400);
            //var fail = authLogs.Count - success;
            var stats = await baseQuery
    .Where(l => l.MethodName == "OtpAuthenticate")
    .GroupBy(l => l.Exception == null) // true => success
    .Select(g => new { IsSuccess = g.Key, Count = g.Count() })
    .ToListAsync();

            var success = stats.FirstOrDefault(x => x.IsSuccess)?.Count ?? 0;
            var fail = stats.FirstOrDefault(x => !x.IsSuccess)?.Count ?? 0;


            return new StatisticResultDto
            {
                Metric = "OtpStats",
                Data = new Dictionary<string, object>
                {
                    { "sent", sent },
                    { "success", success },
                    { "fail", fail },
                    { "successRate", sent == 0 ? 0 : (double)success / sent }
                }
            };
        }

        public async Task<StatisticResultDto> GetSlowQueries(GetLoginStatsInput input)
        {
            var items = await _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .OrderByDescending(l => l.ExecutionDuration)
                .Take(input.Top)
                .Select(l => new SlowQueryDto
                {
                    ServiceName = l.ServiceName,
                    MethodName = l.MethodName,
                    ExecutionDuration = l.ExecutionDuration
                })
                .ToListAsync();

            return new StatisticResultDto
            {
                Metric = "SlowQueries",
                Data = new Dictionary<string, object>
                {
                    { "items", items }
                }
            };
        }

        public async Task<StatisticResultDto> GetQueueStatus(GetLoginStatsInput input)
        {
            var baseQuery = _fcmQueueRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, q => q.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, q => q.CreationTime <= input.EndDate.Value);

            var sentDelays = await baseQuery
                .Where(q => q.IsSent)
                .Select(q => (int?)EF.Functions.DateDiffSecond(q.CreationTime, q.SentTime))
                .ToListAsync();

            var success = sentDelays.Count;
            var pending = await baseQuery.CountAsync(q => !q.IsSent);
            var avgDelay = sentDelays.Where(d => d.HasValue).Any() ? sentDelays.Where(d => d.HasValue).Average(d => d.Value) : 0;

            return new StatisticResultDto
            {
                Metric = "QueueStatus",
                Data = new Dictionary<string, object>
                {
                    { "success", success },
                    { "pending", pending },
                    { "averageDelaySeconds", avgDelay }
                }
            };
        }

        public async Task<StatisticResultDto> GetStorageCapacity(GetLoginStatsInput input)
        {
            var posts = await _postRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, p => p.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.CreationTime <= input.EndDate.Value)
                .Select(p => new
                {
                    p.CreationTime,
                    OrgName = p.PostGroupFk.OrganizationFk.OrganizationName,
                    p.PostFile,
                    p.PostFile2,
                    p.PostFile3,
                    p.PostFile4,
                    p.PostFile5,
                    p.PostFile6,
                    p.PostFile7,
                    p.PostFile8,
                    p.PostFile9,
                    p.PostFile10,
                    p.PdfFile
                })
                .ToListAsync();

            var allIds = posts
                .SelectMany(p => new Guid?[] { p.PostFile, p.PostFile2, p.PostFile3, p.PostFile4, p.PostFile5, p.PostFile6, p.PostFile7, p.PostFile8, p.PostFile9, p.PostFile10, p.PdfFile })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var sizes = await _binaryObjectRepository.GetAll()
                .Where(b => allIds.Contains(b.Id))
                .Select(b => new { b.Id, Size = b.Bytes.Length })
                .ToDictionaryAsync(x => x.Id, x => (long)x.Size);

            long total = 0;
            var periodDict = new Dictionary<DateTime, long>();
            var orgDict = new Dictionary<string, long>();

            foreach (var p in posts)
            {
                var ids = new Guid?[] { p.PostFile, p.PostFile2, p.PostFile3, p.PostFile4, p.PostFile5, p.PostFile6, p.PostFile7, p.PostFile8, p.PostFile9, p.PostFile10, p.PdfFile };
                long size = ids.Where(id => id.HasValue && sizes.ContainsKey(id.Value)).Sum(id => sizes[id.Value]);
                total += size;

                var period = Truncate(p.CreationTime, input.Interval);
                periodDict[period] = periodDict.ContainsKey(period) ? periodDict[period] + size : size;

                var org = p.OrgName ?? "Unknown";
                orgDict[org] = orgDict.ContainsKey(org) ? orgDict[org] + size : size;
            }

            var growth = periodDict
                .OrderBy(g => g.Key)
                .Select(g => new CountPerPeriodDto { PeriodStart = g.Key, Count = (int)(g.Value / 1_000_000) })
                .ToList();

            var topOrgs = orgDict
                .Select(g => new KeyValueStatDto { Key = g.Key, Value = (int)(g.Value / 1_000_000) })
                .OrderByDescending(g => g.Value)
                .Take(input.Top)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "StorageCapacity",
                Data = new Dictionary<string, object>
                {
                    { "totalSizeMB", (int)(total / 1_000_000) },
                    { "growth", growth },
                    { "topOrganizations", topOrgs }
                }
            };
        }

        private static (string Browser, string OS, string Version) ParseUserAgent(string agent)
        {
            if (string.IsNullOrEmpty(agent))
                return ("Unknown", "Unknown", "");

            var ua = agent.ToLowerInvariant();
            string browser;
            if (ua.Contains("chrome")) browser = "Chrome";
            else if (ua.Contains("firefox")) browser = "Firefox";
            else if (ua.Contains("safari") && !ua.Contains("chrome")) browser = "Safari";
            else if (ua.Contains("edge")) browser = "Edge";
            else if (ua.Contains("msie") || ua.Contains("trident")) browser = "IE";
            else browser = "Other";

            string os;
            if (ua.Contains("windows")) os = "Windows";
            else if (ua.Contains("android")) os = "Android";
            else if (ua.Contains("iphone") || ua.Contains("ipad")) os = "iOS";
            else if (ua.Contains("mac os")) os = "macOS";
            else if (ua.Contains("linux")) os = "Linux";
            else os = "Other";

            string version = "";
            var marker = browser.ToLower() + "/";
            var idx = ua.IndexOf(marker);
            if (idx >= 0)
            {
                idx += marker.Length;
                var end = ua.IndexOf(' ', idx);
                if (end < 0) end = ua.Length;
                version = agent.Substring(idx, end - idx);
            }

            return (browser, os, version);
        }

        public async Task<StatisticResultDto> GetClientDistribution(GetLoginStatsInput input)
        {
            var agents = await _auditLogRepository.GetAll()
                .WhereIf(input.StartDate.HasValue, l => l.ExecutionTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, l => l.ExecutionTime <= input.EndDate.Value)
                .Select(l => l.BrowserInfo)
                .ToListAsync();

            var groups = agents
                .Select(ParseUserAgent)
                .GroupBy(a => new { a.Browser, a.OS, a.Version })
                .Select(g => new ClientDistributionDto
                {
                    Browser = g.Key.Browser,
                    OperatingSystem = g.Key.OS,
                    Version = g.Key.Version,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(input.Top)
                .ToList();

            return new StatisticResultDto
            {
                Metric = "ClientDistribution",
                Data = new Dictionary<string, object>
                {
                    { "items", groups }
                }
            };
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Chamran.Deed.EntityFrameworkCore;

namespace Chamran.Deed.HealthChecks
{
    public class DeedDbContextHealthCheck : IHealthCheck
    {
        private readonly DatabaseCheckHelper _checkHelper;

        public DeedDbContextHealthCheck(DatabaseCheckHelper checkHelper)
        {
            _checkHelper = checkHelper;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_checkHelper.Exist("db"))
            {
                return Task.FromResult(HealthCheckResult.Healthy("DeedDbContext connected to database."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("DeedDbContext could not connect to database"));
        }
    }
}

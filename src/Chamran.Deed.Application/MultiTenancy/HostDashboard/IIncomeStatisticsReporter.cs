using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chamran.Deed.MultiTenancy.HostDashboard.Dto;

namespace Chamran.Deed.MultiTenancy.HostDashboard
{
    public interface IIncomeStatisticsService
    {
        Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
            ChartDateInterval dateInterval);
    }
}
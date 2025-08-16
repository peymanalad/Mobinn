using System;

namespace Chamran.Deed.Statistics.Dto
{
    public class GetLoginStatsInput
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public StatisticsInterval Interval { get; set; } = StatisticsInterval.Daily;
        public int Top { get; set; } = 10;
    }
}
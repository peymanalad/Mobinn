using System;

namespace Chamran.Deed.Statistics.Dto
{
    public class LoginStatsItemDto
    {
        public DateTime PeriodStart { get; set; }
        public int Total { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public double SuccessRate { get; set; }
    }
}
using System.Collections.Generic;

namespace Chamran.Deed.Statistics.Dto
{
    public class LoginStatsDto
    {
        public int Total { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public double SuccessRate { get; set; }
        public IReadOnlyList<LoginStatsItemDto> Items { get; set; }
    }
}
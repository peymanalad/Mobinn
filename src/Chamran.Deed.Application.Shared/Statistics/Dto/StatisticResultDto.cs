using System.Collections.Generic;

namespace Chamran.Deed.Statistics.Dto
{
    /// <summary>
    /// Generic container DTO for statistics endpoints.
    /// </summary>
    public class StatisticResultDto
    {
        public string Metric { get; set; }
        public string Message { get; set; }
        public IDictionary<string, object> Data { get; set; }
    }
}
using System;

namespace Chamran.Deed.Statistics.Dto
{
    public class ServiceLatencyDto
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public double Average { get; set; }
        public double P95 { get; set; }
        public double P99 { get; set; }
    }
}
namespace Chamran.Deed.Statistics.Dto
{
    public class ServiceErrorRateDto
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }

        public int Total { get; set; }
        public int Success { get; set; }
        public int Fail { get; set; }

        public double ErrorRate { get; set; }

        public string TopException { get; set; }

        public int? Error4xx { get; set; }
        public int? Error5xx { get; set; }
        public double? Rate4xx { get; set; }
        public double? Rate5xx { get; set; }
    }
}

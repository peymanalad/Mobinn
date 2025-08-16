using System;

namespace Chamran.Deed.Statistics.Dto
{
    public class ServiceCallCountDto
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public int Count { get; set; }
    }
}
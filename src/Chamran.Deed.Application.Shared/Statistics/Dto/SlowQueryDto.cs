using System;

namespace Chamran.Deed.Statistics.Dto
{
    public class SlowQueryDto
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public int ExecutionDuration { get; set; }
    }
}
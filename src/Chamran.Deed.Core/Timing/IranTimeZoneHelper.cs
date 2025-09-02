using System;

namespace Chamran.Deed.Timing
{
    public static class IranTimeZoneHelper
    {
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
    }
}
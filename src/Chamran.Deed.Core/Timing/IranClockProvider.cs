using System;
using Abp.Timing;

namespace Chamran.Deed.Timing
{
    public class IranClockProvider : IClockProvider
    {
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);

        public DateTimeKind Kind => DateTimeKind.Unspecified;

        public bool SupportsMultipleTimezone => true;

        public DateTime Normalize(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, IranTimeZone);
            }

            return TimeZoneInfo.ConvertTime(dateTime, IranTimeZone);
        }
    }
}
using System;
using System.Globalization;

namespace Chamran.Deed.Web.Controllers.Stimulsoft
{
    public class ConversionFunctions
    {
        public static string ConvertToPersianCalendar(DateTime georgianDate)
        {
            // Create an instance of the PersianCalendar class
            var persianCalendar = new PersianCalendar();

            // Calculate the Persian year, month, and day
            var pYear = persianCalendar.GetYear(georgianDate);
            var pMonth = persianCalendar.GetMonth(georgianDate);
            var pDay = persianCalendar.GetDayOfMonth(georgianDate);

            // Format the Persian date as a string in the format "yyyy/MM/dd"
            var persianDate = $"{pYear:0000}/{pMonth:00}/{pDay:00}";

            return persianDate;
        }
    }
}

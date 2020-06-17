using System;

namespace Bcss.Reference.Domain
{
    public static class DomainExtensions
    {
        // Here, we use an extension method within the shared Domain layer to capture the business format for dates.
        public static string ToMonthDayYearDate(this DateTime date)
        {
            return $"{date.Month}/{date.Day}/{date.Year}";
        }
    }
}
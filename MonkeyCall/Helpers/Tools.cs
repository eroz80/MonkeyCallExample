using System;
using Foundation;

namespace MonkeyCall.Helpers
{
    public static class Tools
    {
        public static DateTime ConvertNsDateToDateTime(NSDate date)
        {
            DateTime newDate = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return newDate.AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate ConvertDateTimeToNSDate(DateTime date)
        {
            DateTime newDate = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - newDate).TotalSeconds);
        }
    }
}

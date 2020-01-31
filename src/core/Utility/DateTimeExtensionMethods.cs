using System;
namespace core.Utility
{
    public static class DateTimeExtensionMethods
    {
        public static string ToFileDateTime(this DateTime when)
        {
            return when.ToString("dd MMMM yyyy HH:mm:ss");
        }
    }
}

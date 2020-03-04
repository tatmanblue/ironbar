using System;
namespace core.Utility
{
    public static class DateTimeExtensionMethods
    {
        /// <summary>
        /// Converts date/time into format acceptable as a filename.
        /// Found in core.Utility.DateTimeExtensionMethods
        /// </summary>
        /// <seealso cref="core.Utility.DateTimeExtensionMethods"/>
        /// <param name="when"></param>
        /// <returns></returns>
        public static string ToFileDateTime(this DateTime when)
        {
            return when.ToString("dd MMMM yyyy HHmmss");
        }
    }
}

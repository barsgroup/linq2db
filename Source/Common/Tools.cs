using System.Collections;

namespace LinqToDB.Common
{
    using LinqToDB.Properties;

    public static class Tools
    {
        [StringFormatMethod("format")]
        public static string Args(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool IsNullOrEmpty(this ICollection array)
        {
            return array == null || array.Count == 0;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}

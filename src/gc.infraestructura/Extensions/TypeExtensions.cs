namespace System
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TypeExtensions
    {
        public static bool ToBoolean(this String s)
        {
            bool.TryParse(s, out bool value);
            return value;
        }

        public static bool ToBoolean(this String s, bool returnValue)
        {
            return bool.TryParse(s, out bool value) ? value : returnValue;
        }

        public static bool? ToBooleanOrNull(this String s)
        {
            if (bool.TryParse(s, out bool value)) return value;
            return null;
        }

        public static string ToCleanString(this String s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s.Replace("\0", "").Replace("\r\n", " ");
        }

        public static short ToShort(this String s)
        {
            short.TryParse(s, out short value);
            return value;
        }


        public static short ToShort(this String s, short returnValue)
        {
            return short.TryParse(s, out short value) ? value : returnValue;
        }

        public static short? ToShortOrNull(this String s)
        {
            if (short.TryParse(s, out short value)) return value;
            return null;
        }


        public static int ToInt(this String s)
        {
            int.TryParse(s, out int value);
            return value;
        }

        public static int ToInt(this String s, int returnValue)
        {
            return int.TryParse(s, out int value) ? value : returnValue;
        }

        public static int? ToIntOrNull(this String s)
        {
            if (int.TryParse(s, out int value)) return value;
            return null;
        }


        public static long ToLong(this String s)
        {
            long.TryParse(s, out long value);
            return value;
        }

        public static long ToLong(this String s, long returnValue)
        {
            return long.TryParse(s, out long value) ? value : returnValue;
        }

        public static long? ToLongOrNull(this String s)
        {
            if (long.TryParse(s, out long value)) return value;
            return null;
        }


        public static float ToFloat(this String s)
        {
            float.TryParse(s, out float value);
            return value;
        }

        public static float ToFloat(this String s, float returnValue)
        {
            return float.TryParse(s, out float value) ? value : returnValue;
        }

        public static float? ToFloatOrNull(this String s)
        {
            if (float.TryParse(s, out float value)) return value;
            return null;
        }


        public static double ToDouble(this String s)
        {
            double.TryParse(s, out double value);
            return value;
        }

        public static double ToDouble(this String s, double returnValue)
        {
            return double.TryParse(s, out double value) ? value : returnValue;
        }

        public static double? ToDoubleOrNull(this String s)
        {
            if (double.TryParse(s, out double value)) return value;
            return null;
        }


        public static decimal ToDecimal(this String s)
        {
            decimal.TryParse(s, out decimal value);
            return value;
        }

        public static decimal ToDecimal(this String s, decimal returnValue)
        {
            return decimal.TryParse(s, out decimal value) ? value : returnValue;
        }

        public static decimal? ToDecimalOrNull(this String s)
        {
            if (decimal.TryParse(s, out decimal value)) return value;
            return null;
        }


        public static DateTime ToDateTime(this String s)
        {
            return ToDateTime(s, new DateTime());
        }

       
        public static DateTime? ToDateTimeFromTicks(this String s)
        {
            try
            {
                return new DateTime(s.ToLong());
            }
            catch 
            {
                return null;
            }
        }

        public static DateTime ToDateTime(this String s, DateTime returnValue)
        {
            return DateTime.TryParse(s, out DateTime value) ? value : returnValue;
        }

        public static DateTime? ToDateTimeOrNull(this String s)
        {
            return DateTime.TryParse(s, out DateTime value) ? (DateTime?)value : null;
        }

        public static string ToStringJSDate(this DateTime d)
        {
            return $"{d.Year}-{d.Month.ToString().PadLeft(2,'0')}-{d.Day.ToString().PadLeft(2, '0')}";
        }

        public static string ToStringYYYYMMDD(this DateTime d)
        {
            return $"{d.Year}{d.Month.ToString().PadLeft(2, '0')}{d.Day.ToString().PadLeft(2, '0')}";
        }

        public static Guid ToGuid(this String s)
        {
            return ToGuid(s, Guid.Empty);
        }

        public static Guid ToGuid(this String s, Guid returnValue)
        {
            try
            {
                return new Guid(s);
            }
            catch
            {
                return returnValue;
            }
        }

        public static Guid? ToGuidOrNull(this String s)
        {
            try
            {
                return string.IsNullOrEmpty(s) ? (Guid?)null : new Guid(s);
            }
            catch
            {
                return null;
            }
        }


        public static string ToHtml(this String s)
        {
            s = s.Replace(Environment.NewLine, "<br />");
            return s;
        }

        public static string ToText(this String s)
        {
            s = s.Replace("<br />", Environment.NewLine);
            return s;
        }


        public static bool IsNullOrEmpty(this string input)
        {
            return String.IsNullOrWhiteSpace(input);
        }

        public static string Fill(this string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static bool ContainsIgnoreCase(this string left, string right)
        {
            var pattern = new Regex(right, RegexOptions.IgnoreCase);
            return pattern.IsMatch(left);
        }

        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return String.Compare(left, right, true) == 0;
        }

        public static bool IsDbNull(this object value)
        {
            return Convert.IsDBNull(value);
        }

        public static decimal Redondear(this decimal valor, int decimales)
        {
            return Math.Round(valor, decimales);
        }

        public static decimal Redondear(this decimal valor)
        {
            return Math.Round(valor, 2);
        }

        public static decimal Redondear(this string valor)
        {
            var numero = valor.ToDecimal();
            return Math.Round(numero, 2);
        }

        public static decimal Redondear(this string valor, int decimales)
        {
            var numero = valor.ToDecimal();
            return Math.Round(numero, decimales);
        }

        public static List<string>? SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }
    }
}

using System.Globalization;

namespace RetailCorrector
{
    internal static class ParserConstants
    {
        public const string DATE_FORMAT = "dd'-'MM'-'yyyy";
        public static IFormatProvider FORMAT_PROVIDER = CultureInfo.InvariantCulture;
    }
}
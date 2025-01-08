using System.Globalization;

namespace FileServer;

internal static class FileSizeFormat
{
    static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
    {
        if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

        var i = 0;
        var dValue = (decimal)value;
        while (Math.Round(dValue, decimalPlaces) >= 1000)
        {
            dValue /= 1024;
            i++;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
    }
}

using System.Globalization;
using System.Text.RegularExpressions;

namespace ModLoader.Core;

/// <summary>
/// Достаёт Workshop Published File ID из числа или ссылки filedetails/?id=…
/// </summary>
public static class WorkshopIdParser
{
    private static readonly Regex IdInUrl = new(
        @"[?&]id=(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool TryParse(string? input, out ulong publishedFileId)
    {
        publishedFileId = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();

        var match = IdInUrl.Match(trimmed);
        if (match.Success &&
            ulong.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out publishedFileId) &&
            publishedFileId > 0)
            return true;

        if (ulong.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out publishedFileId) &&
            publishedFileId > 0)
            return true;

        publishedFileId = 0;
        return false;
    }
}

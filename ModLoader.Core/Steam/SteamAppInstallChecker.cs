using System.Text.RegularExpressions;

namespace ModLoader.Core.Steam;

/// <summary>
/// Проверка установки игры по appmanifest_*.acf в steamapps (включая libraryfolders.vdf).
/// </summary>
public static class SteamAppInstallChecker
{
    private static readonly Regex PathLine = new(
        "\"path\"\\s+\"([^\"]+)\"",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool IsInstalled(string? steamPath, uint appId)
    {
        if (string.IsNullOrWhiteSpace(steamPath) || !Directory.Exists(steamPath) || appId == 0)
            return false;

        foreach (var libraryRoot in EnumerateLibraryRoots(steamPath))
        {
            var manifest = Path.Combine(libraryRoot, "steamapps", $"appmanifest_{appId}.acf");
            if (File.Exists(manifest))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Корни Steam-библиотек: основной SteamPath + пути из libraryfolders.vdf.
    /// </summary>
    public static IEnumerable<string> EnumerateLibraryRoots(string steamPath)
    {
        if (string.IsNullOrWhiteSpace(steamPath) || !Directory.Exists(steamPath))
            yield break;

        yield return steamPath;

        var foldersFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(foldersFile))
            yield break;

        string text;
        try
        {
            text = File.ReadAllText(foldersFile);
        }
        catch
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamPath };
        foreach (Match match in PathLine.Matches(text))
        {
            var path = match.Groups[1].Value.Replace("\\\\", "\\").Trim();
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                continue;
            if (seen.Add(path))
                yield return path;
        }
    }
}

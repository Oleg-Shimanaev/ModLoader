namespace ModLoader.Core.Steam;

/// <summary>
/// Поиск steamcmd.exe: в указанной папке (+1 уровень вглубь) и по стандартным кандидатам.
/// Без абсолютных путей конкретного ПК — только SpecialFolder / BaseDirectory.
/// </summary>
public static class SteamCmdLocator
{
    public const string ExeName = "steamcmd.exe";

    /// <summary>
    /// Ищет steamcmd.exe в папке и в непосредственных подпапках (1 уровень).
    /// </summary>
    public static string? ResolveInFolder(string? folderOrExe)
    {
        if (string.IsNullOrWhiteSpace(folderOrExe))
            return null;

        var trimmed = folderOrExe.Trim().Trim('"');

        if (File.Exists(trimmed) &&
            trimmed.EndsWith(ExeName, StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if (!Directory.Exists(trimmed))
            return null;

        var direct = Path.Combine(trimmed, ExeName);
        if (File.Exists(direct))
            return direct;

        try
        {
            foreach (var sub in Directory.EnumerateDirectories(trimmed))
            {
                var nested = Path.Combine(sub, ExeName);
                if (File.Exists(nested))
                    return nested;
            }
        }
        catch
        {
            // нет доступа к подпапкам
        }

        return null;
    }

    /// <summary>
    /// Кандидаты автопоиска (существование файла проверяет вызывающий код / FindFirstExisting).
    /// </summary>
    public static IReadOnlyList<string> GetAutoSearchCandidates(string? appBaseDirectory = null)
    {
        var list = new List<string>();

        void Add(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                list.Add(path);
        }

        Add(SteamCmdInstaller.DefaultExePath);

        var baseDir = appBaseDirectory ?? AppContext.BaseDirectory;
        if (!string.IsNullOrWhiteSpace(baseDir))
        {
            Add(Path.Combine(baseDir, ExeName));
            Add(Path.Combine(baseDir, "steamcmd", ExeName));
        }

        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localApp))
            Add(Path.Combine(localApp, "steamcmd", ExeName));

        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(pf))
            Add(Path.Combine(pf, "SteamCMD", ExeName));

        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(pf86))
            Add(Path.Combine(pf86, "SteamCMD", ExeName));

        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(profile))
            Add(Path.Combine(profile, "steamcmd", ExeName));

        return list;
    }

    public static string? FindFirstExisting(
        IEnumerable<string> candidates,
        Func<string, bool>? fileExists = null)
    {
        fileExists ??= File.Exists;
        foreach (var c in candidates)
        {
            if (fileExists(c))
                return c;
        }

        return null;
    }
}

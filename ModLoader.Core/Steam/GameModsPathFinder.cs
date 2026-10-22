namespace ModLoader.Core.Steam;

/// <summary>
/// Ищет типичные папки модов Door Kickers 2 (KillHouse) и Steam libraries.
/// </summary>
public static class GameModsPathFinder
{
    public const uint DoorKickers2AppId = 1239080;

    public const string DoorKickers2ModsRelative = @"steamapps\common\DoorKickers2\mods";

    public static string GetLocalAppDataDoorKickers2Root() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KillHouseGames",
            "DoorKickers2");

    public static string GetLocalModsUploadPath() =>
        Path.Combine(GetLocalAppDataDoorKickers2Root(), "mods_upload");

    public static string GetLocalModsPath() =>
        Path.Combine(GetLocalAppDataDoorKickers2Root(), "mods");

    /// <summary>
    /// Кандидаты для FolderBrowser.InitialDirectory (существующие каталоги, по приоритету).
    /// Официально для upload — mods_upload в LocalAppData; затем Local mods; затем Steam install\mods.
    /// </summary>
    public static IReadOnlyList<string> GetFolderPickerCandidates(string? steamPath, uint preferredAppId = DoorKickers2AppId)
    {
        var list = new List<string>();
        void AddIfExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return;
            if (!list.Contains(path, StringComparer.OrdinalIgnoreCase))
                list.Add(path);
        }

        AddIfExists(GetLocalModsUploadPath());
        AddIfExists(GetLocalModsPath());

        if (!string.IsNullOrWhiteSpace(steamPath))
        {
            foreach (var root in SteamAppInstallChecker.EnumerateLibraryRoots(steamPath))
            {
                if (preferredAppId == DoorKickers2AppId || preferredAppId == 0)
                    AddIfExists(Path.Combine(root, DoorKickers2ModsRelative));

                AddIfExists(Path.Combine(root, @"steamapps\common\DoorKickers\mods"));
            }
        }

        return list;
    }

    /// <summary>Лучший стартовый каталог для диалога выбора папки.</summary>
    public static string? SuggestInitialDirectory(string? steamPath, uint preferredAppId = DoorKickers2AppId) =>
        GetFolderPickerCandidates(steamPath, preferredAppId).FirstOrDefault();

    /// <summary>
    /// Найти существующую папку модов в Steam libraries (install path).
    /// </summary>
    public static string? FindModsFolder(string? steamPath, uint preferredAppId = DoorKickers2AppId)
    {
        if (string.IsNullOrWhiteSpace(steamPath))
            return null;

        var relative = preferredAppId == 248610
            ? @"steamapps\common\DoorKickers\mods"
            : DoorKickers2ModsRelative;

        foreach (var root in SteamAppInstallChecker.EnumerateLibraryRoots(steamPath))
        {
            var candidate = Path.Combine(root, relative);
            if (Directory.Exists(candidate))
                return candidate;
        }

        // fallback: другая известная игра
        foreach (var root in SteamAppInstallChecker.EnumerateLibraryRoots(steamPath))
        {
            var other = preferredAppId == 248610
                ? Path.Combine(root, DoorKickers2ModsRelative)
                : Path.Combine(root, @"steamapps\common\DoorKickers\mods");
            if (Directory.Exists(other))
                return other;
        }

        return null;
    }

    /// <summary>Типичное имя превью в шаблонах KillHouse.</summary>
    public const string DefaultPreviewFileName = "mod_image.jpg";

    public static string? FindDefaultPreviewImage(string? modFolder)
    {
        if (string.IsNullOrWhiteSpace(modFolder) || !Directory.Exists(modFolder))
            return null;

        var jpg = Path.Combine(modFolder, DefaultPreviewFileName);
        if (File.Exists(jpg))
            return jpg;

        foreach (var name in new[] { "mod_image.png", "preview.jpg", "preview.png" })
        {
            var path = Path.Combine(modFolder, name);
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}

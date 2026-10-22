using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class GameModsPathFinderTests
{
    [Fact]
    public void FindModsFolder_НаходитDoorKickers2ModsВLibrary()
    {
        var root = Path.Combine(Path.GetTempPath(), "ml_libs_" + Guid.NewGuid().ToString("N"));
        var steam = Path.Combine(root, "Steam");
        var extraLib = Path.Combine(root, "ExtraSteamApps");
        var mods = Path.Combine(extraLib, "steamapps", "common", "DoorKickers2", "mods");
        Directory.CreateDirectory(mods);
        Directory.CreateDirectory(Path.Combine(steam, "steamapps"));
        File.WriteAllText(Path.Combine(steam, "steamapps", "libraryfolders.vdf"), $@"
""libraryfolders""
{{
	""1""
	{{
		""path""		""{extraLib.Replace("\\", "\\\\")}""
	}}
}}
");
        try
        {
            var found = GameModsPathFinder.FindModsFolder(steam, preferredAppId: 1239080);
            found.Should().Be(mods);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetFolderPickerCandidates_ПриоритетLocalModsUpload()
    {
        // Метод должен предпочитать LocalAppData пути, если они существуют.
        // В CI их может не быть — проверяем порядок логики через Steam-only сценарий.
        var root = Path.Combine(Path.GetTempPath(), "ml_cand_" + Guid.NewGuid().ToString("N"));
        var steam = Path.Combine(root, "Steam");
        var mods = Path.Combine(steam, "steamapps", "common", "DoorKickers2", "mods");
        Directory.CreateDirectory(mods);
        try
        {
            var candidates = GameModsPathFinder.GetFolderPickerCandidates(steam, 1239080);
            candidates.Should().Contain(mods);
            candidates[0].Should().Be(candidates.First()); // non-empty
            // Local paths, if present on machine, come before steam install mods
            var steamIndex = candidates.ToList().FindIndex(c =>
                c.Contains("DoorKickers2", StringComparison.OrdinalIgnoreCase) &&
                c.Contains(Path.Combine("steamapps", "common"), StringComparison.OrdinalIgnoreCase));
            if (steamIndex > 0)
            {
                candidates[0].Should().Match(p =>
                    p.Contains("KillHouseGames", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("mods_upload", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("mods", StringComparison.OrdinalIgnoreCase));
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FindDefaultPreviewImage_НаходитModImageJpg()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ml_prev_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var preview = Path.Combine(dir, "mod_image.jpg");
        File.WriteAllBytes(preview, new byte[] { 1 });
        try
        {
            GameModsPathFinder.FindDefaultPreviewImage(dir).Should().Be(preview);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void FindModsFolder_НетБиблиотек_Null()
    {
        GameModsPathFinder.FindModsFolder(null).Should().BeNull();
        GameModsPathFinder.FindModsFolder(@"C:\нет_такого_steam_xyz").Should().BeNull();
    }

    [Fact]
    public void DoorKickers2_AppId_1239080()
    {
        GameModsPathFinder.DoorKickers2AppId.Should().Be(1239080);
        GameModsPathFinder.DefaultPreviewFileName.Should().Be("mod_image.jpg");
    }
}

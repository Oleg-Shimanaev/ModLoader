using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class SteamAppInstallCheckerTests
{
    [Fact]
    public void IsInstalled_НетПапкиSteam_False()
    {
        SteamAppInstallChecker.IsInstalled(null, 248610).Should().BeFalse();
        SteamAppInstallChecker.IsInstalled(@"C:\нет_такого_steam_xyz", 248610).Should().BeFalse();
    }

    [Fact]
    public void IsInstalled_ЕстьAppManifest_True()
    {
        var root = Path.Combine(Path.GetTempPath(), "ModLoaderSteam_" + Guid.NewGuid().ToString("N"));
        var steamapps = Path.Combine(root, "steamapps");
        Directory.CreateDirectory(steamapps);
        File.WriteAllText(Path.Combine(steamapps, "appmanifest_248610.acf"), "\"AppState\"{}");
        try
        {
            SteamAppInstallChecker.IsInstalled(root, 248610).Should().BeTrue();
            SteamAppInstallChecker.IsInstalled(root, 1239080).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsInstalled_БиблиотекаИзLibraryFolders_True()
    {
        var root = Path.Combine(Path.GetTempPath(), "ModLoaderSteam_" + Guid.NewGuid().ToString("N"));
        var lib = Path.Combine(Path.GetTempPath(), "ModLoaderLib_" + Guid.NewGuid().ToString("N"));
        var steamapps = Path.Combine(root, "steamapps");
        var libApps = Path.Combine(lib, "steamapps");
        Directory.CreateDirectory(steamapps);
        Directory.CreateDirectory(libApps);
        File.WriteAllText(Path.Combine(libApps, "appmanifest_1239080.acf"), "\"AppState\"{}");
        File.WriteAllText(Path.Combine(steamapps, "libraryfolders.vdf"), $@"
""libraryfolders""
{{
	""0""
	{{
		""path""		""{lib.Replace("\\", "\\\\")}""
	}}
}}
");
        try
        {
            SteamAppInstallChecker.IsInstalled(root, 1239080).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, true);
            Directory.Delete(lib, true);
        }
    }
}

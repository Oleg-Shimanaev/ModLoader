using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class SteamCmdLocatorTests : IDisposable
{
    private readonly string _root;

    public SteamCmdLocatorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ModLoaderTests", "SteamCmdLocator", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // ignore cleanup
        }
    }

    [Fact]
    public void ResolveInFolder_ExeНапрямую_ВозвращаетПуть()
    {
        var exe = Path.Combine(_root, "steamcmd.exe");
        File.WriteAllText(exe, "x");

        SteamCmdLocator.ResolveInFolder(exe).Should().Be(exe);
    }

    [Fact]
    public void ResolveInFolder_ExeВКорнеПапки_Находит()
    {
        var exe = Path.Combine(_root, "steamcmd.exe");
        File.WriteAllText(exe, "x");

        SteamCmdLocator.ResolveInFolder(_root).Should().Be(exe);
    }

    [Fact]
    public void ResolveInFolder_ExeНаОдинУровеньВглубь_Находит()
    {
        var nested = Path.Combine(_root, "SteamCMD");
        Directory.CreateDirectory(nested);
        var exe = Path.Combine(nested, "steamcmd.exe");
        File.WriteAllText(exe, "x");

        SteamCmdLocator.ResolveInFolder(_root).Should().Be(exe);
    }

    [Fact]
    public void ResolveInFolder_НетExe_ВозвращаетNull()
    {
        SteamCmdLocator.ResolveInFolder(_root).Should().BeNull();
    }

    [Fact]
    public void ResolveInFolder_ПустаяСтрока_Null()
    {
        SteamCmdLocator.ResolveInFolder("  ").Should().BeNull();
        SteamCmdLocator.ResolveInFolder(null).Should().BeNull();
    }

    [Fact]
    public void GetAutoSearchCandidates_СодержитLocalAppDataModLoaderИProgramFiles()
    {
        var appBase = Path.Combine(_root, "app");
        Directory.CreateDirectory(appBase);

        var candidates = SteamCmdLocator.GetAutoSearchCandidates(appBase);

        candidates.Should().Contain(SteamCmdInstaller.DefaultExePath);
        candidates.Should().Contain(Path.Combine(appBase, "steamcmd.exe"));
        candidates.Should().Contain(Path.Combine(appBase, "steamcmd", "steamcmd.exe"));

        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(pf))
            candidates.Should().Contain(Path.Combine(pf, "SteamCMD", "steamcmd.exe"));

        candidates.Should().NotContain(c => c.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase));
        candidates.Should().OnlyContain(c =>
            c.Contains("steamcmd.exe", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FindFirstExisting_ВозвращаетПервыйСуществующий()
    {
        var a = Path.Combine(_root, "a", "steamcmd.exe");
        var b = Path.Combine(_root, "b", "steamcmd.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(b)!);
        File.WriteAllText(b, "x");

        var found = SteamCmdLocator.FindFirstExisting(new[] { a, b });

        found.Should().Be(b);
    }
}

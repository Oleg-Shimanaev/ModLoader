using System.Diagnostics;
using Microsoft.Win32;

namespace ModLoader.Core.Steam;

/// <summary>
/// Реальная работа со Steam на Windows.
/// </summary>
public sealed class SteamClient : ISteamClient
{
    private string? _steamCmdOverride;

    public bool IsRunning()
    {
        try
        {
            return Process.GetProcessesByName("steam").Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool LaunchSteam()
    {
        var steamExe = FindSteamExe();
        if (steamExe is null)
            return false;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = steamExe,
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? FindSteamPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var path = key?.GetValue("SteamPath") as string;
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                return path.Replace('/', '\\');
        }
        catch
        {
            // ignore
        }

        var defaults = new[]
        {
            @"C:\Program Files (x86)\Steam",
            @"C:\Program Files\Steam"
        };

        return defaults.FirstOrDefault(Directory.Exists);
    }

    public void SetSteamCmdPath(string? pathOrFolder)
    {
        _steamCmdOverride = SteamCmdLocator.ResolveInFolder(pathOrFolder);
    }

    public string? FindSteamCmdPath()
    {
        if (!string.IsNullOrWhiteSpace(_steamCmdOverride) && File.Exists(_steamCmdOverride))
            return _steamCmdOverride;

        var auto = SteamCmdLocator.FindFirstExisting(SteamCmdLocator.GetAutoSearchCandidates());
        if (auto is not null)
            return auto;

        var steam = FindSteamPath();
        if (steam is not null)
        {
            var inSteam = SteamCmdLocator.ResolveInFolder(steam);
            if (inSteam is not null)
                return inSteam;
        }

        return null;
    }

    public bool IsAppInstalled(uint appId) =>
        SteamAppInstallChecker.IsInstalled(FindSteamPath(), appId);

    public bool LaunchAppWithPublishArgs(uint appId, string modFolderPath)
    {
        var steamExe = FindSteamExe();
        if (steamExe is null)
            return false;

        var args = $"-applaunch {appId} -publish \"{modFolderPath}\"";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = steamExe,
                Arguments = args,
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public SteamCmdLaunchAttempt LaunchSteamCmdWorkshopBuild(string vdfPath, string username)
    {
        var steamCmd = FindSteamCmdPath();
        var exeExists = !string.IsNullOrWhiteSpace(steamCmd) && File.Exists(steamCmd);
        if (!exeExists)
            return SteamCmdLaunchAttempt.Fail(SteamCmdLaunchFailureKind.NotFound);

        if (string.IsNullOrWhiteSpace(username) || !File.Exists(vdfPath))
            return SteamCmdLaunchAttempt.Fail(SteamCmdLaunchFailureKind.StartFailed);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = steamCmd,
                Arguments = $"+login {username} +workshop_build_item \"{vdfPath}\" +quit",
                UseShellExecute = true
            });

            if (process is null)
            {
                return SteamCmdLaunchAttempt.Fail(
                    SteamCmdLaunchDiagnostics.Classify(
                        exeExists: true,
                        startSucceeded: false));
            }

            // Если процесс сразу умер — консоль не успела остаться открытой.
            var exited = process.WaitForExit(SteamCmdLaunchDiagnostics.DefaultGraceMs);
            var kind = SteamCmdLaunchDiagnostics.Classify(
                exeExists: true,
                startSucceeded: true,
                processExitedWithinGrace: exited);

            return kind == SteamCmdLaunchFailureKind.None
                ? SteamCmdLaunchAttempt.Ok()
                : SteamCmdLaunchAttempt.Fail(kind);
        }
        catch
        {
            return SteamCmdLaunchAttempt.Fail(SteamCmdLaunchFailureKind.StartFailed);
        }
    }

    private string? FindSteamExe()
    {
        var path = FindSteamPath();
        if (path is null)
            return null;

        var exe = Path.Combine(path, "steam.exe");
        return File.Exists(exe) ? exe : null;
    }
}

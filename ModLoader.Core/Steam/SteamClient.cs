using System.Diagnostics;
using Microsoft.Win32;

namespace ModLoader.Core.Steam;

/// <summary>
/// Реальная работа со Steam на Windows.
/// </summary>
public sealed class SteamClient : ISteamClient
{
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

    public string? FindSteamCmdPath()
    {
        var steam = FindSteamPath();
        if (steam is not null)
        {
            var candidate = Path.Combine(steam, "steamcmd.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        var common = new[]
        {
            @"C:\steamcmd\steamcmd.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "steamcmd", "steamcmd.exe")
        };

        return common.FirstOrDefault(File.Exists);
    }

    public bool LaunchAppWithPublishArgs(uint appId, string modFolderPath)
    {
        var steamExe = FindSteamExe();
        if (steamExe is null)
            return false;

        // Как в гайде: -publish <путь к папке мода>
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

    private string? FindSteamExe()
    {
        var path = FindSteamPath();
        if (path is null)
            return null;

        var exe = Path.Combine(path, "steam.exe");
        return File.Exists(exe) ? exe : null;
    }
}

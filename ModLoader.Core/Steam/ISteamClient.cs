namespace ModLoader.Core.Steam;

/// <summary>
/// Абстракция над Steam-клиентом (мокается в тестах).
/// </summary>
public interface ISteamClient
{
    bool IsRunning();
    bool LaunchSteam();
    string? FindSteamPath();
    string? FindSteamCmdPath();

    /// <summary>
    /// Запускает игру через Steam с аргументом -publish (как в гайде Door Kickers).
    /// </summary>
    bool LaunchAppWithPublishArgs(uint appId, string modFolderPath);
}

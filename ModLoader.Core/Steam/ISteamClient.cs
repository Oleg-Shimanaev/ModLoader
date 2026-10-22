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
    /// Задать путь к steamcmd.exe (или к папке, где он лежит).
    /// </summary>
    void SetSteamCmdPath(string? pathOrFolder);

    /// <summary>
    /// Есть ли локально установленная игра с этим AppID (appmanifest).
    /// </summary>
    bool IsAppInstalled(uint appId);

    /// <summary>
    /// Legacy: запуск игры через Steam с -publish (не основной путь).
    /// </summary>
    bool LaunchAppWithPublishArgs(uint appId, string modFolderPath);

    /// <summary>
    /// Запускает steamcmd workshop_build_item для создания/обновления Workshop item.
    /// </summary>
    bool LaunchSteamCmdWorkshopBuild(string vdfPath, string username);
}

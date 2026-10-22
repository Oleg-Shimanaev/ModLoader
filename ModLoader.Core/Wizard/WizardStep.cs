namespace ModLoader.Core.Wizard;

public enum WizardStep
{
    ModFolder = 0,
    GameAppId = 1,
    PublishKind = 2,
    TitleDescription = 3,
    Preview = 4,
    UploadMethod = 5,
    Confirm = 6
}

public enum PublishKind
{
    CreateNew = 0,
    UpdateExisting = 1
}

/// <summary>
/// Способ загрузки в Workshop.
/// </summary>
public enum UploadMethod
{
    /// <summary>Steamworks → сессия запущенного Steam-клиента (без пароля).</summary>
    SteamClient = 0,

    /// <summary>Запасной путь: steamcmd + логин (пароль в окне steamcmd).</summary>
    SteamCmd = 1
}

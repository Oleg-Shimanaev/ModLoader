using ModLoader.Core.Models;

namespace ModLoader.Core.Steam;

public sealed class SteamReadyResult
{
    public bool IsReady { get; init; }
    public bool NeedsLaunchPermission { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class PublishResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? VdfPath { get; init; }
    public string? SuggestedCommand { get; init; }
}

public sealed class PublishService
{
    private readonly ISteamClient _steam;
    private readonly WorkshopVdfGenerator _vdfGenerator;

    public PublishService(ISteamClient steam, WorkshopVdfGenerator? vdfGenerator = null)
    {
        _steam = steam;
        _vdfGenerator = vdfGenerator ?? new WorkshopVdfGenerator();
    }

    public SteamReadyResult EnsureSteamReady(bool allowLaunch)
    {
        if (_steam.IsRunning())
        {
            return new SteamReadyResult
            {
                IsReady = true,
                NeedsLaunchPermission = false,
                Message = "Steam уже запущен."
            };
        }

        if (!allowLaunch)
        {
            return new SteamReadyResult
            {
                IsReady = false,
                NeedsLaunchPermission = true,
                Message = "Steam не запущен. Разрешите запуск Steam и нажмите «Загрузить» снова."
            };
        }

        if (!_steam.LaunchSteam())
        {
            return new SteamReadyResult
            {
                IsReady = false,
                NeedsLaunchPermission = false,
                Message = "Не удалось запустить Steam. Откройте Steam вручную и попробуйте снова."
            };
        }

        // Даём клиенту время подняться; в UI можно подождать и проверить снова.
        if (!_steam.IsRunning())
        {
            return new SteamReadyResult
            {
                IsReady = true,
                NeedsLaunchPermission = false,
                Message = "Steam запускается. Подождите несколько секунд и продолжайте."
            };
        }

        return new SteamReadyResult
        {
            IsReady = true,
            NeedsLaunchPermission = false,
            Message = "Steam запущен."
        };
    }

    /// <summary>
    /// Основной путь по гайду Door Kickers: запуск игры с -publish «папка мода».
    /// Дальше загрузку делает сам Steam / игра.
    /// </summary>
    public PublishResult PublishViaGame(UploadParameters parameters)
    {
        if (!_steam.IsRunning())
        {
            return new PublishResult
            {
                Success = false,
                Message = "Steam не запущен. Нажмите «Запустить Steam», дождитесь входа и попробуйте снова."
            };
        }

        var ok = _steam.LaunchAppWithPublishArgs(parameters.AppId, parameters.ModFolder);
        if (!ok)
        {
            return new PublishResult
            {
                Success = false,
                Message = "Не удалось запустить игру через Steam. Проверьте AppID и что игра установлена."
            };
        }

        return new PublishResult
        {
            Success = true,
            Message =
                "Игра запущена в режиме публикации. Дальше следуйте окнам Steam / игры. " +
                "Не закрывайте Steam, пока загрузка не закончится."
        };
    }

    /// <summary>
    /// Альтернатива: подготовить VDF и команду для steamcmd workshop_build_item.
    /// </summary>
    public PublishResult PrepareSteamCmdUpload(UploadParameters parameters, ulong publishedFileId = 0)
    {
        var steamCmd = _steam.FindSteamCmdPath();
        if (string.IsNullOrWhiteSpace(steamCmd))
        {
            return new PublishResult
            {
                Success = false,
                Message =
                    "Не найден steamcmd.exe. Установите SteamCMD или используйте загрузку через игру (кнопка «Загрузить»)."
            };
        }

        var vdfPath = _vdfGenerator.WriteToTempFile(parameters, publishedFileId);
        var command = $"\"{steamCmd}\" +login anonymous +workshop_build_item \"{vdfPath}\" +quit";

        return new PublishResult
        {
            Success = true,
            VdfPath = vdfPath,
            SuggestedCommand = command,
            Message = "Файл для steamcmd подготовлен. Обычно проще нажать «Загрузить» (через игру)."
        };
    }
}

using ModLoader.Core.Models;
using ModLoader.Core.Wizard;

namespace ModLoader.Core.Steam;

public sealed class SteamReadyResult
{
    public bool IsReady { get; init; }
    public bool NeedsLaunchPermission { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class SteamUiStatus
{
    public bool IsRunning { get; init; }
    public bool ShowLaunchButton { get; init; }
    public string StatusText { get; init; } = string.Empty;
}

public sealed class SteamCmdUiStatus
{
    public bool IsFound { get; init; }
    public bool ShowPickButton { get; init; }
    public string? Path { get; init; }
    public string StatusText { get; init; } = string.Empty;
}

public sealed class PublishResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? VdfPath { get; init; }
    public string? SuggestedCommand { get; init; }
    public ulong? PublishedFileId { get; init; }
    public bool OfferFaq { get; init; }
}

public sealed class PublishService
{
    private readonly ISteamClient _steam;
    private readonly WorkshopVdfGenerator _vdfGenerator;
    private readonly IWorkshopPublisher _workshopPublisher;

    public PublishService(
        ISteamClient steam,
        WorkshopVdfGenerator? vdfGenerator = null,
        IWorkshopPublisher? workshopPublisher = null)
    {
        _steam = steam;
        _vdfGenerator = vdfGenerator ?? new WorkshopVdfGenerator();
        _workshopPublisher = workshopPublisher ?? new SteamworksWorkshopPublisher();
    }

    public SteamUiStatus GetSteamUiStatus()
    {
        if (_steam.IsRunning())
        {
            return new SteamUiStatus
            {
                IsRunning = true,
                ShowLaunchButton = false,
                StatusText = "Steam ✓"
            };
        }

        return new SteamUiStatus
        {
            IsRunning = false,
            ShowLaunchButton = true,
            StatusText = "Steam не запущен"
        };
    }

    public SteamCmdUiStatus GetSteamCmdUiStatus()
    {
        var path = _steam.FindSteamCmdPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            return new SteamCmdUiStatus
            {
                IsFound = true,
                ShowPickButton = false,
                Path = path,
                StatusText = "steamcmd ✓"
            };
        }

        return new SteamCmdUiStatus
        {
            IsFound = false,
            ShowPickButton = true,
            Path = null,
            StatusText = "steamcmd не найден"
        };
    }

    public void SetSteamCmdPath(string? pathOrFolder) =>
        _steam.SetSteamCmdPath(pathOrFolder);

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
                Message = "Steam не запущен. Разрешите запуск или откройте Steam сами."
            };
        }

        if (!_steam.LaunchSteam())
        {
            return new SteamReadyResult
            {
                IsReady = false,
                NeedsLaunchPermission = false,
                Message = "Не удалось запустить Steam. Откройте его вручную. См. FAQ."
            };
        }

        if (!_steam.IsRunning())
        {
            return new SteamReadyResult
            {
                IsReady = true,
                NeedsLaunchPermission = false,
                Message = "Steam запускается. Войдите в аккаунт и продолжайте."
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
    /// Публикация выбранным способом: Steam-клиент (Steamworks) или steamcmd.
    /// </summary>
    public Task<PublishResult> PublishAsync(
        UploadParameters parameters,
        UploadMethod method,
        string? steamUsername = null,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default) =>
        method == UploadMethod.SteamClient
            ? PublishViaSteamClientAsync(parameters, progress, cancellationToken)
            : Task.FromResult(PublishViaSteamCmd(parameters, steamUsername));

    public async Task<PublishResult> PublishViaSteamClientAsync(
        UploadParameters parameters,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!_steam.IsRunning())
        {
            return new PublishResult
            {
                Success = false,
                Message = "Запустите Steam и войдите в аккаунт. См. FAQ.",
                OfferFaq = true
            };
        }

        if (!_workshopPublisher.IsAvailable)
        {
            return new PublishResult
            {
                Success = false,
                Message =
                    "Steamworks недоступен (нет steam_api64.dll). Выберите «Через steamcmd» или переустановите ModLoader. См. FAQ.",
                OfferFaq = true
            };
        }

        var result = await _workshopPublisher
            .PublishAsync(parameters, progress, cancellationToken)
            .ConfigureAwait(false);

        return new PublishResult
        {
            Success = result.Success,
            Message = result.Message,
            PublishedFileId = result.PublishedFileId > 0 ? result.PublishedFileId : null,
            OfferFaq = result.OfferFaq
        };
    }

    /// <summary>
    /// Legacy: запуск игры с -publish. Не используется основным Publish.
    /// </summary>
    public PublishResult PublishViaGame(UploadParameters parameters)
    {
        if (!_steam.IsRunning())
        {
            return new PublishResult
            {
                Success = false,
                Message = "Steam не запущен."
            };
        }

        if (!_steam.IsAppInstalled(parameters.AppId))
        {
            return new PublishResult
            {
                Success = false,
                Message = PublishMessages.GameNotInstalled
            };
        }

        var ok = _steam.LaunchAppWithPublishArgs(parameters.AppId, parameters.ModFolder);
        if (!ok)
        {
            return new PublishResult
            {
                Success = false,
                Message = PublishMessages.LaunchFailed
            };
        }

        return new PublishResult
        {
            Success = true,
            Message = "Игра запущена в режиме публикации (legacy)."
        };
    }

    public PublishResult PublishViaSteamCmd(UploadParameters parameters, string? steamUsername)
    {
        if (string.IsNullOrWhiteSpace(steamUsername))
        {
            return new PublishResult
            {
                Success = false,
                Message = "Нужен логин Steam. Пароль и Steam Guard введите в окне steamcmd. См. FAQ."
            };
        }

        var prepared = PrepareSteamCmdUpload(parameters, steamUsername.Trim());
        if (!prepared.Success || prepared.VdfPath is null)
            return prepared;

        if (!_steam.LaunchSteamCmdWorkshopBuild(prepared.VdfPath, steamUsername.Trim()))
        {
            return new PublishResult
            {
                Success = false,
                VdfPath = prepared.VdfPath,
                SuggestedCommand = prepared.SuggestedCommand,
                Message =
                    "Не удалось запустить steamcmd. Укажите папку steamcmd или выполните команду вручную:\n" +
                    (prepared.SuggestedCommand ?? "") +
                    "\n\nСм. FAQ."
            };
        }

        var mode = parameters.IsUpdate
            ? "обновления Workshop item (ID " + parameters.PublishedFileId + ")"
            : "публикации нового мода";

        return new PublishResult
        {
            Success = true,
            VdfPath = prepared.VdfPath,
            SuggestedCommand = prepared.SuggestedCommand,
            Message =
                "Запущен steamcmd для " + mode + ".\n\n" +
                "В окне steamcmd введите пароль (и код Steam Guard, если попросит). " +
                "Дождитесь сообщения об успехе. Игра не запускается. См. FAQ."
        };
    }

    /// <summary>
    /// Подготовить VDF и команду для steamcmd workshop_build_item.
    /// </summary>
    public PublishResult PrepareSteamCmdUpload(
        UploadParameters parameters,
        string? steamUsername = null)
    {
        var steamCmd = _steam.FindSteamCmdPath();
        if (string.IsNullOrWhiteSpace(steamCmd))
        {
            return new PublishResult
            {
                Success = false,
                Message =
                    "Не найден steamcmd.exe. Нажмите «Указать папку steamcmd…» или «Скачать и установить steamcmd». См. FAQ."
            };
        }

        var vdfPath = _vdfGenerator.WriteToTempFile(parameters);
        var login = string.IsNullOrWhiteSpace(steamUsername) ? "ВАШ_ЛОГИН" : steamUsername.Trim();
        var command = $"\"{steamCmd}\" +login {login} +workshop_build_item \"{vdfPath}\" +quit";

        return new PublishResult
        {
            Success = true,
            VdfPath = vdfPath,
            SuggestedCommand = command,
            Message = parameters.IsUpdate
                ? "VDF для обновления через steamcmd подготовлен."
                : "VDF для нового мода через steamcmd подготовлен."
        };
    }
}

using ModLoader.Core.Models;
using Steamworks;
using Steamworks.Ugc;
using FpPublishResult = Steamworks.Ugc.PublishResult;
using FpSteamClient = Steamworks.SteamClient;

namespace ModLoader.Core.Steam;

/// <summary>
/// Загрузка Workshop через Facepunch.Steamworks к уже запущенному Steam-клиенту
/// (тот же аккаунт, без пароля в UI).
/// </summary>
public sealed class SteamworksWorkshopPublisher : IWorkshopPublisher
{
    private readonly string _appBaseDirectory;
    private readonly Func<uint, bool, bool> _init;
    private readonly Action _shutdown;
    private readonly Func<bool> _isLoggedOn;
    private readonly Func<UploadParameters, IProgress<float>?, CancellationToken, Task<FpPublishResult>> _submit;

    public SteamworksWorkshopPublisher(
        string? appBaseDirectory = null,
        Func<uint, bool, bool>? init = null,
        Action? shutdown = null,
        Func<bool>? isLoggedOn = null,
        Func<UploadParameters, IProgress<float>?, CancellationToken, Task<FpPublishResult>>? submit = null)
    {
        _appBaseDirectory = appBaseDirectory ?? AppContext.BaseDirectory;
        _init = init ?? DefaultInit;
        _shutdown = shutdown ?? DefaultShutdown;
        _isLoggedOn = isLoggedOn ?? DefaultIsLoggedOn;
        _submit = submit ?? DefaultSubmitAsync;
    }

    private static void DefaultShutdown()
    {
        if (FpSteamClient.IsValid)
            FpSteamClient.Shutdown();
    }

    private static bool DefaultIsLoggedOn() =>
        FpSteamClient.IsValid && FpSteamClient.IsLoggedOn;

    public bool IsAvailable =>
        File.Exists(Path.Combine(_appBaseDirectory, "steam_api64.dll")) ||
        File.Exists(Path.Combine(_appBaseDirectory, "steam_api.dll"));

    public async Task<WorkshopPublishResult> PublishAsync(
        UploadParameters parameters,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureSteamAppIdFile(parameters.AppId);

            if (!_init(parameters.AppId, true))
            {
                return Fail(
                    "Не удалось подключиться к Steam-клиенту. Запустите Steam и войдите в аккаунт. См. FAQ.",
                    offerFaq: true);
            }

            if (!_isLoggedOn())
            {
                return Fail(
                    "Steam запущен, но вход не выполнен. Войдите в Steam и повторите. См. FAQ.",
                    offerFaq: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var result = await _submit(parameters, progress, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                var msg = result.NeedsWorkshopAgreement
                    ? "Нужно принять соглашение Workshop в Steam (оверлей / страница Workshop). См. FAQ."
                    : "Не удалось загрузить через Steam-клиент (" + result.Result + "). См. FAQ.";
                return Fail(msg, offerFaq: true);
            }

            var id = (ulong)result.FileId;
            var mode = parameters.IsUpdate ? "обновлён" : "опубликован";
            return new WorkshopPublishResult
            {
                Success = true,
                PublishedFileId = id,
                Message = parameters.IsUpdate
                    ? $"Мод {mode} через Steam-клиент (ID {id})."
                    : $"Мод {mode} через Steam-клиент. Workshop ID: {id}."
            };
        }
        catch (OperationCanceledException)
        {
            return Fail("Загрузка отменена.");
        }
        catch (Exception)
        {
            return Fail(
                "Ошибка Steamworks. Нужен запущенный Steam и право публиковать в Workshop. См. FAQ.",
                offerFaq: true);
        }
        finally
        {
            try
            {
                _shutdown();
            }
            catch
            {
                // ignore shutdown errors
            }
        }
    }

    private void EnsureSteamAppIdFile(uint appId)
    {
        try
        {
            var path = Path.Combine(_appBaseDirectory, "steam_appid.txt");
            File.WriteAllText(path, appId.ToString());
        }
        catch
        {
            // Init всё равно получит appId аргументом
        }
    }

    private static bool DefaultInit(uint appId, bool asyncCallbacks)
    {
        try
        {
            if (FpSteamClient.IsValid)
                FpSteamClient.Shutdown();

            FpSteamClient.Init(appId, asyncCallbacks);
            return FpSteamClient.IsValid;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<FpPublishResult> DefaultSubmitAsync(
        UploadParameters parameters,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Editor editor = parameters.IsUpdate
            ? new Editor(parameters.PublishedFileId)
            : Editor.NewCommunityFile;

        editor = editor
            .ForAppId(parameters.AppId)
            .WithTitle(parameters.Title)
            .WithDescription(parameters.Description)
            .WithChangeLog(parameters.ChangeNote)
            .WithContent(parameters.ModFolder);

        editor = parameters.Visibility switch
        {
            1 => editor.WithFriendsOnlyVisibility(),
            2 => editor.WithPrivateVisibility(),
            _ => editor.WithPublicVisibility()
        };

        if (!string.IsNullOrWhiteSpace(parameters.PreviewImagePath))
            editor = editor.WithPreviewFile(parameters.PreviewImagePath);

        return await editor.SubmitAsync(progress).ConfigureAwait(false);
    }

    private static WorkshopPublishResult Fail(string message, bool offerFaq = false) =>
        new()
        {
            Success = false,
            Message = message,
            OfferFaq = offerFaq
        };
}

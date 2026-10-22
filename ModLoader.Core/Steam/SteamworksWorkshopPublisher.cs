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
                    SteamReachabilityDiagnostics.UserMessage(SteamReachabilityFailureKind.InitFailed),
                    offerFaq: true);
            }

            if (!_isLoggedOn())
            {
                return Fail(
                    SteamReachabilityDiagnostics.UserMessage(SteamReachabilityFailureKind.NotLoggedOn),
                    offerFaq: true);
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Нет связи / зависание API — не молчим, отдаём таймаут.
            linked.CancelAfter(TimeSpan.FromSeconds(90));

            var result = await _submit(parameters, progress, linked.Token).ConfigureAwait(false);

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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Fail("Загрузка отменена.");
        }
        catch (OperationCanceledException)
        {
            return Fail(
                SteamReachabilityDiagnostics.UserMessage(SteamReachabilityFailureKind.Timeout),
                offerFaq: true);
        }
        catch (Exception)
        {
            return Fail(
                SteamReachabilityDiagnostics.UserMessage(SteamReachabilityFailureKind.Unknown),
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

            TryDeleteSteamAppIdFile();
        }
    }

    /// <summary>
    /// Facepunch/Steam API may read steam_appid.txt next to the process.
    /// Always rewrite from the wizard AppID — never ship a fixed test file in the zip.
    /// </summary>
    private void EnsureSteamAppIdFile(uint appId)
    {
        try
        {
            var path = Path.Combine(_appBaseDirectory, "steam_appid.txt");
            File.WriteAllText(path, appId.ToString());
        }
        catch
        {
            // Init still receives appId as argument
        }
    }

    private void TryDeleteSteamAppIdFile()
    {
        try
        {
            var path = Path.Combine(_appBaseDirectory, "steam_appid.txt");
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // leftover file is harmless; next publish overwrites
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

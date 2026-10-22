namespace ModLoader.Core.Steam;

public sealed class SteamCmdInstallResult
{
    public bool Success { get; init; }
    public string? SteamCmdExePath { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool OfferFaq { get; init; }
    public SteamCmdBootstrapFailureKind FailureKind { get; init; }
}

/// <summary>
/// Скачивает официальный steamcmd.zip во временную папку, распаковывает
/// в выбранную (или дефолтную) папку и запускает bootstrap (+quit).
/// </summary>
public sealed class SteamCmdInstaller
{
    public const string OfficialZipUrl =
        "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

    public const string ExeFileName = "steamcmd.exe";

    public static readonly TimeSpan DefaultBootstrapRetryDelay = TimeSpan.FromSeconds(2);

    private readonly IHttpDownloader _http;
    private readonly IArchiveExtractor _zip;
    private readonly IProcessRunner _process;
    private readonly IFileSystem _fs;
    private readonly string _tempDirectory;
    private readonly string _installDirectory;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly TimeSpan _bootstrapRetryDelay;

    public SteamCmdInstaller(
        IHttpDownloader http,
        IArchiveExtractor zip,
        IProcessRunner process,
        IFileSystem? fileSystem = null,
        string? tempDirectory = null,
        string? installDirectory = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        TimeSpan? bootstrapRetryDelay = null)
    {
        _http = http;
        _zip = zip;
        _process = process;
        _fs = fileSystem ?? new RealFileSystem();
        _tempDirectory = tempDirectory ?? DefaultTempDirectory;
        _installDirectory = installDirectory ?? DefaultInstallDirectory;
        _delay = delay ?? ((ts, ct) => Task.Delay(ts, ct));
        _bootstrapRetryDelay = bootstrapRetryDelay ?? DefaultBootstrapRetryDelay;
    }

    public static string DefaultInstallDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ModLoader",
            "steamcmd");

    public static string DefaultTempDirectory =>
        Path.Combine(Path.GetTempPath(), "ModLoader");

    public static string DefaultExePath =>
        Path.Combine(DefaultInstallDirectory, ExeFileName);

    public static SteamCmdInstaller CreateDefault() =>
        new(new HttpClientDownloader(), new ZipArchiveExtractor(), new ProcessRunner());

    /// <summary>
    /// Создаёт дефолтную папку установки (если нет) и возвращает её путь.
    /// </summary>
    public static string EnsureDefaultInstallDirectoryExists(IFileSystem? fileSystem = null)
    {
        var fs = fileSystem ?? new RealFileSystem();
        var dir = DefaultInstallDirectory;
        fs.EnsureDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Короткий статус: для дефолта — шаблон %LOCALAPPDATA%, иначе выбранный путь.
    /// </summary>
    public static string FormatInstallSuccessMessage(string installDirectory)
    {
        if (IsSameDirectory(installDirectory, DefaultInstallDirectory))
            return "steamcmd установлен: %LOCALAPPDATA%\\ModLoader\\steamcmd\\";

        return "steamcmd установлен: " + installDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
    }

    public Task<SteamCmdInstallResult> InstallAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default) =>
        InstallAsync(installDirectory: null, progress, cancellationToken);

    public async Task<SteamCmdInstallResult> InstallAsync(
        string? installDirectory,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var targetDir = string.IsNullOrWhiteSpace(installDirectory)
            ? _installDirectory
            : installDirectory.Trim();

        try
        {
            progress?.Report("Скачивание steamcmd…");
            _fs.EnsureDirectory(_tempDirectory);
            var zipPath = Path.Combine(_tempDirectory, "steamcmd.zip");
            await _http.DownloadFileAsync(new Uri(OfficialZipUrl), zipPath, cancellationToken)
                .ConfigureAwait(false);

            progress?.Report("Распаковка…");
            _fs.EnsureDirectory(targetDir);
            _zip.ExtractZip(zipPath, targetDir);

            var exePath = Path.Combine(targetDir, ExeFileName);
            if (!_fs.FileExists(exePath))
            {
                return Fail("После распаковки нет steamcmd.exe. См. FAQ.", offerFaq: true);
            }

            progress?.Report("Идёт установка…");
            var first = await RunBootstrapAsync(exePath, targetDir, cancellationToken)
                .ConfigureAwait(false);
            var failure = SteamCmdBootstrapDiagnostics.Classify(first.CombinedOutput, first.ExitCode);

            if (failure != SteamCmdBootstrapFailureKind.None)
            {
                progress?.Report("Повтор установки…");
                await _delay(_bootstrapRetryDelay, cancellationToken).ConfigureAwait(false);
                var second = await RunBootstrapAsync(exePath, targetDir, cancellationToken)
                    .ConfigureAwait(false);
                failure = SteamCmdBootstrapDiagnostics.Classify(second.CombinedOutput, second.ExitCode);
            }

            if (failure != SteamCmdBootstrapFailureKind.None)
            {
                return Fail(
                    SteamCmdBootstrapDiagnostics.UserMessage(failure),
                    offerFaq: true,
                    failure);
            }

            if (!_fs.FileExists(exePath))
            {
                return Fail(
                    SteamCmdBootstrapDiagnostics.BootstrapFailedUserMessage,
                    offerFaq: true,
                    SteamCmdBootstrapFailureKind.BootstrapFailed);
            }

            _fs.DeleteFileIfExists(zipPath);

            return new SteamCmdInstallResult
            {
                Success = true,
                SteamCmdExePath = exePath,
                Message = FormatInstallSuccessMessage(targetDir)
            };
        }
        catch (OperationCanceledException)
        {
            return Fail("Установка отменена.");
        }
        catch (Exception)
        {
            return Fail("Не удалось установить steamcmd. См. FAQ.", offerFaq: true);
        }
    }

    private Task<ProcessRunResult> RunBootstrapAsync(
        string exePath,
        string workingDirectory,
        CancellationToken cancellationToken) =>
        _process.RunAsync(exePath, "+quit", workingDirectory, cancellationToken);

    private static bool IsSameDirectory(string a, string b)
    {
        try
        {
            var na = Path.GetFullPath(a)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var nb = Path.GetFullPath(b)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static SteamCmdInstallResult Fail(
        string message,
        bool offerFaq = false,
        SteamCmdBootstrapFailureKind failureKind = SteamCmdBootstrapFailureKind.None) =>
        new()
        {
            Success = false,
            SteamCmdExePath = null,
            Message = message,
            OfferFaq = offerFaq,
            FailureKind = failureKind
        };
}

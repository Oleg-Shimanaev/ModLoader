using FluentAssertions;
using ModLoader.Core.Steam;
using Moq;

namespace ModLoader.Tests;

public class SteamCmdInstallerTests
{
    private readonly Mock<IHttpDownloader> _http = new();
    private readonly Mock<IArchiveExtractor> _zip = new();
    private readonly Mock<IProcessRunner> _process = new();
    private readonly Mock<IFileSystem> _fs = new();
    private readonly string _tempDir;
    private readonly string _installDir;
    private readonly List<TimeSpan> _delays = new();
    private readonly SteamCmdInstaller _sut;

    public SteamCmdInstallerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ModLoaderTests", "temp");
        _installDir = Path.Combine(Path.GetTempPath(), "ModLoaderTests", "install");
        _sut = new SteamCmdInstaller(
            _http.Object,
            _zip.Object,
            _process.Object,
            _fs.Object,
            _tempDir,
            _installDir,
            delay: (ts, _) =>
            {
                _delays.Add(ts);
                return Task.CompletedTask;
            },
            bootstrapRetryDelay: TimeSpan.FromMilliseconds(1));
    }

    private static ProcessRunResult OkRun() => new() { ExitCode = 0, CombinedOutput = "Loading Steam API...OK." };

    private static ProcessRunResult HttpError0Run() => new()
    {
        ExitCode = 1,
        CombinedOutput =
            "Download failed: http error 0\n" +
            "Package file steamcmd_win32.zip missing or incorrect size\n" +
            "Error: Download of package (steamcmd_win32) failed after 0 bytes (0)."
    };

    [Fact]
    public void DefaultInstallDirectory_ПодLocalAppData_ModLoader_steamcmd()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ModLoader",
            "steamcmd");

        SteamCmdInstaller.DefaultInstallDirectory.Should().Be(expected);
    }

    [Fact]
    public void DefaultTempDirectory_ПодGetTempPath_ModLoader()
    {
        var expected = Path.Combine(Path.GetTempPath(), "ModLoader");

        SteamCmdInstaller.DefaultTempDirectory.Should().Be(expected);
    }

    [Fact]
    public void OfficialZipUrl_ОфициальныйWindowsZip()
    {
        SteamCmdInstaller.OfficialZipUrl.Should().Be(
            "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
    }

    [Fact]
    public void FormatInstallSuccessMessage_ДефолтныйПуть_ШаблономLocalAppData()
    {
        SteamCmdInstaller.FormatInstallSuccessMessage(SteamCmdInstaller.DefaultInstallDirectory)
            .Should().Be("steamcmd установлен: %LOCALAPPDATA%\\ModLoader\\steamcmd\\");
    }

    [Fact]
    public void FormatInstallSuccessMessage_КастомныйПуть_ПоказываетЕго()
    {
        var custom = Path.Combine(Path.GetTempPath(), "ModLoaderTests", "elsewhere");

        SteamCmdInstaller.FormatInstallSuccessMessage(custom)
            .Should().Be($"steamcmd установлен: {custom}");
    }

    [Fact]
    public void EnsureDefaultInstallDirectoryExists_СоздаётИВозвращаетДефолт()
    {
        var path = SteamCmdInstaller.EnsureDefaultInstallDirectoryExists(_fs.Object);

        path.Should().Be(SteamCmdInstaller.DefaultInstallDirectory);
        _fs.Verify(f => f.EnsureDirectory(SteamCmdInstaller.DefaultInstallDirectory), Times.Once);
    }

    [Fact]
    public async Task InstallAsync_Успех_Скачивает_Распаковывает_ЗапускаетBootstrap_ВозвращаетExe()
    {
        var zipPath = Path.Combine(_tempDir, "steamcmd.zip");
        var exePath = Path.Combine(_installDir, "steamcmd.exe");
        var statuses = new List<string>();

        _http
            .Setup(h => h.DownloadFileAsync(
                It.Is<Uri>(u => u.ToString() == SteamCmdInstaller.OfficialZipUrl),
                zipPath,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fs.Setup(f => f.FileExists(exePath)).Returns(true);
        _process
            .Setup(p => p.RunAsync(exePath, "+quit", _installDir, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OkRun());

        var result = await _sut.InstallAsync(new Progress<string>(statuses.Add));

        result.Success.Should().BeTrue();
        result.SteamCmdExePath.Should().Be(exePath);
        result.Message.Should().Be(SteamCmdInstaller.FormatInstallSuccessMessage(_installDir));
        result.OfferFaq.Should().BeFalse();

        _fs.Verify(f => f.EnsureDirectory(_tempDir), Times.Once);
        _fs.Verify(f => f.EnsureDirectory(_installDir), Times.Once);
        _zip.Verify(z => z.ExtractZip(zipPath, _installDir), Times.Once);
        _process.Verify(
            p => p.RunAsync(exePath, "+quit", _installDir, It.IsAny<CancellationToken>()),
            Times.Once);
        _fs.Verify(f => f.DeleteFileIfExists(zipPath), Times.Once);
        _delays.Should().BeEmpty();

        statuses.Should().Contain(s => s.Contains("Скачивание", StringComparison.OrdinalIgnoreCase));
        statuses.Should().Contain(s => s.Contains("Распаковка", StringComparison.OrdinalIgnoreCase));
        statuses.Should().Contain(s => s.Contains("установка", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InstallAsync_СInjectableПутём_РаспаковываетВВыбраннуюПапку()
    {
        var customDir = Path.Combine(Path.GetTempPath(), "ModLoaderTests", "chosen-install");
        var zipPath = Path.Combine(_tempDir, "steamcmd.zip");
        var exePath = Path.Combine(customDir, "steamcmd.exe");

        _http
            .Setup(h => h.DownloadFileAsync(
                It.IsAny<Uri>(),
                zipPath,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fs.Setup(f => f.FileExists(exePath)).Returns(true);
        _process
            .Setup(p => p.RunAsync(exePath, "+quit", customDir, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OkRun());

        var result = await _sut.InstallAsync(installDirectory: customDir);

        result.Success.Should().BeTrue();
        result.SteamCmdExePath.Should().Be(exePath);
        result.Message.Should().Be(SteamCmdInstaller.FormatInstallSuccessMessage(customDir));

        _fs.Verify(f => f.EnsureDirectory(customDir), Times.Once);
        _zip.Verify(z => z.ExtractZip(zipPath, customDir), Times.Once);
        _process.Verify(
            p => p.RunAsync(exePath, "+quit", customDir, It.IsAny<CancellationToken>()),
            Times.Once);
        // конструкторный _installDir не трогаем
        _fs.Verify(f => f.EnsureDirectory(_installDir), Times.Never);
    }

    [Fact]
    public async Task InstallAsync_НетExeПослеРаспаковки_КороткаяОшибкаСFaq()
    {
        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fs.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

        var result = await _sut.InstallAsync();

        result.Success.Should().BeFalse();
        result.SteamCmdExePath.Should().BeNull();
        result.Message.Should().Contain("FAQ");
        result.OfferFaq.Should().BeTrue();
        _process.Verify(
            p => p.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InstallAsync_ОшибкаСкачивания_КороткаяОшибкаСFaq()
    {
        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network"));

        var result = await _sut.InstallAsync();

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("FAQ");
        result.OfferFaq.Should().BeTrue();
        _zip.Verify(z => z.ExtractZip(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InstallAsync_ИсключениеПриBootstrap_КороткаяОшибкаСFaq()
    {
        var exePath = Path.Combine(_installDir, "steamcmd.exe");
        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fs.Setup(f => f.FileExists(exePath)).Returns(true);
        _process
            .Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        var result = await _sut.InstallAsync();

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("FAQ");
        result.OfferFaq.Should().BeTrue();
    }

    [Fact]
    public async Task InstallAsync_HttpError0_ПовторяетBootstrapОдинРаз_ИДаётСетевоеСообщение()
    {
        var exePath = Path.Combine(_installDir, "steamcmd.exe");
        var statuses = new List<string>();

        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fs.Setup(f => f.FileExists(exePath)).Returns(true);
        _process
            .Setup(p => p.RunAsync(exePath, "+quit", _installDir, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HttpError0Run());

        var result = await _sut.InstallAsync(new Progress<string>(statuses.Add));

        result.Success.Should().BeFalse();
        result.FailureKind.Should().Be(SteamCmdBootstrapFailureKind.HttpErrorZero);
        result.OfferFaq.Should().BeTrue();
        result.Message.Should().Be(SteamCmdBootstrapDiagnostics.HttpErrorZeroUserMessage);
        result.Message.Should().Contain("сеть");
        result.Message.Should().Contain("FAQ");

        _process.Verify(
            p => p.RunAsync(exePath, "+quit", _installDir, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _delays.Should().ContainSingle();
        statuses.Should().Contain(s => s.Contains("Повтор", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InstallAsync_HttpError0ЗатемУспех_ВозвращаетУспехПослеRetry()
    {
        var exePath = Path.Combine(_installDir, "steamcmd.exe");
        var calls = 0;

        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fs.Setup(f => f.FileExists(exePath)).Returns(true);
        _process
            .Setup(p => p.RunAsync(exePath, "+quit", _installDir, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                calls++;
                return calls == 1 ? HttpError0Run() : OkRun();
            });

        var result = await _sut.InstallAsync();

        result.Success.Should().BeTrue();
        result.SteamCmdExePath.Should().Be(exePath);
        calls.Should().Be(2);
        _delays.Should().ContainSingle();
    }

    [Fact]
    public async Task InstallAsync_Отмена_СообщаетОбОтмене()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _http.Setup(h => h.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var result = await _sut.InstallAsync(cancellationToken: cts.Token);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("отменен");
        result.OfferFaq.Should().BeFalse();
    }
}

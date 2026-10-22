using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class SteamCmdBootstrapDiagnosticsTests
{
    private const string SampleHttpError0Log =
        """
        Download failed: http error 0
        Package file steamcmd_win32.zip (1) missing or incorrect size
        Error: Download of package (steamcmd_win32) failed after 0 bytes (0).
        """;

    [Fact]
    public void IsHttpErrorZero_ПустойТекст_False()
    {
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero(null).Should().BeFalse();
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero("").Should().BeFalse();
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero("   ").Should().BeFalse();
    }

    [Fact]
    public void IsHttpErrorZero_УспешныйBootstrap_False()
    {
        var ok = "Loading Steam API...OK.\nRedirecting stderr to 'logs\\stderr.txt'\n";
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero(ok).Should().BeFalse();
    }

    [Fact]
    public void IsHttpErrorZero_ТипичныйЛогCdn_True()
    {
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero(SampleHttpError0Log).Should().BeTrue();
    }

    [Fact]
    public void IsHttpErrorZero_ТолькоHttpError0_True()
    {
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero("Download failed: http error 0").Should().BeTrue();
    }

    [Fact]
    public void IsHttpErrorZero_РегистрНеВажен()
    {
        SteamCmdBootstrapDiagnostics.IsHttpErrorZero("DOWNLOAD FAILED: HTTP ERROR 0").Should().BeTrue();
    }

    [Fact]
    public void Classify_HttpError0_ВозвращаетСетевойВидИСообщениеСFaq()
    {
        var kind = SteamCmdBootstrapDiagnostics.Classify(SampleHttpError0Log, exitCode: 1);

        kind.Should().Be(SteamCmdBootstrapFailureKind.HttpErrorZero);
        var message = SteamCmdBootstrapDiagnostics.UserMessage(kind);
        message.Should().Contain("сеть");
        message.Should().Contain("FAQ");
        message.Length.Should().BeLessThan(220);
    }

    [Fact]
    public void Classify_НенулевойКодБезHttpError0_ОбщийBootstrapСбой()
    {
        var kind = SteamCmdBootstrapDiagnostics.Classify("something went wrong", exitCode: 8);

        kind.Should().Be(SteamCmdBootstrapFailureKind.BootstrapFailed);
        SteamCmdBootstrapDiagnostics.UserMessage(kind).Should().Contain("FAQ");
    }

    [Fact]
    public void Classify_НулевойКодБезОшибок_None()
    {
        SteamCmdBootstrapDiagnostics.Classify("Loading Steam API...OK.", exitCode: 0)
            .Should().Be(SteamCmdBootstrapFailureKind.None);
    }

    [Fact]
    public void Classify_HttpError0ДажеПриКоде0_СетевойВид()
    {
        // steamcmd иногда оставляет 0 при битом пакете — ориентируемся на лог
        SteamCmdBootstrapDiagnostics.Classify(SampleHttpError0Log, exitCode: 0)
            .Should().Be(SteamCmdBootstrapFailureKind.HttpErrorZero);
    }
}

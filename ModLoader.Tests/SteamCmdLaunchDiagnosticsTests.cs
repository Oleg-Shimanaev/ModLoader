using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class SteamCmdLaunchDiagnosticsTests
{
    [Fact]
    public void Classify_ExeНет_NotFound()
    {
        SteamCmdLaunchDiagnostics.Classify(exeExists: false, startSucceeded: false)
            .Should().Be(SteamCmdLaunchFailureKind.NotFound);
    }

    [Fact]
    public void Classify_StartНеУдался_StartFailed()
    {
        SteamCmdLaunchDiagnostics.Classify(exeExists: true, startSucceeded: false)
            .Should().Be(SteamCmdLaunchFailureKind.StartFailed);
    }

    [Fact]
    public void Classify_СразуВышел_ExitedImmediately()
    {
        SteamCmdLaunchDiagnostics.Classify(
                exeExists: true,
                startSucceeded: true,
                processExitedWithinGrace: true)
            .Should().Be(SteamCmdLaunchFailureKind.ExitedImmediately);
    }

    [Fact]
    public void Classify_ТаймаутБезВывода_TimeoutNoOutput()
    {
        SteamCmdLaunchDiagnostics.Classify(
                exeExists: true,
                startSucceeded: true,
                timedOutWaitingForOutput: true,
                output: "   ")
            .Should().Be(SteamCmdLaunchFailureKind.TimeoutNoOutput);
    }

    [Fact]
    public void Classify_ПроцессЖив_None()
    {
        SteamCmdLaunchDiagnostics.Classify(
                exeExists: true,
                startSucceeded: true,
                processExitedWithinGrace: false)
            .Should().Be(SteamCmdLaunchFailureKind.None);
    }

    [Theory]
    [InlineData(SteamCmdLaunchFailureKind.NotFound, "steamcmd.exe")]
    [InlineData(SteamCmdLaunchFailureKind.StartFailed, "консоль не открылась")]
    [InlineData(SteamCmdLaunchFailureKind.ExitedImmediately, "сразу закрылся")]
    [InlineData(SteamCmdLaunchFailureKind.TimeoutNoOutput, "таймаут")]
    public void UserMessage_СодержитКлючИFaq(SteamCmdLaunchFailureKind kind, string key)
    {
        var msg = SteamCmdLaunchDiagnostics.UserMessage(kind);

        msg.Should().Contain(key);
        msg.Should().Contain("FAQ");
        msg.Length.Should().BeLessThan(260);
        SteamCmdLaunchDiagnostics.OfferFaq(kind).Should().BeTrue();
    }

    [Fact]
    public void ExitedImmediately_УпоминаетАнтивирусИHttpError0()
    {
        var msg = SteamCmdLaunchDiagnostics.ExitedImmediatelyUserMessage;
        msg.Should().Contain("Антивирус");
        msg.Should().Contain("http error 0");
    }

    [Fact]
    public void Attempt_OkИFail()
    {
        SteamCmdLaunchAttempt.Ok().Success.Should().BeTrue();
        SteamCmdLaunchAttempt.Fail(SteamCmdLaunchFailureKind.StartFailed).Success.Should().BeFalse();
        SteamCmdLaunchAttempt.Fail(SteamCmdLaunchFailureKind.StartFailed).FailureKind
            .Should().Be(SteamCmdLaunchFailureKind.StartFailed);
    }
}

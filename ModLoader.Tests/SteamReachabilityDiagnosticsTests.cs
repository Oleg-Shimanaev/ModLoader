using FluentAssertions;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class SteamReachabilityDiagnosticsTests
{
    [Theory]
    [InlineData(SteamReachabilityFailureKind.NotRunning, "не запущен")]
    [InlineData(SteamReachabilityFailureKind.ApiUnavailable, "steam_api64")]
    [InlineData(SteamReachabilityFailureKind.InitFailed, "Init failed")]
    [InlineData(SteamReachabilityFailureKind.NotLoggedOn, "вход")]
    [InlineData(SteamReachabilityFailureKind.Timeout, "таймаут")]
    [InlineData(SteamReachabilityFailureKind.Unknown, "Steam")]
    public void UserMessage_СодержитКлючИFaq(SteamReachabilityFailureKind kind, string key)
    {
        var msg = SteamReachabilityDiagnostics.UserMessage(kind);

        msg.Should().Contain(key);
        msg.Should().Contain("FAQ");
        msg.Length.Should().BeLessThan(280);
        SteamReachabilityDiagnostics.OfferFaq(kind).Should().BeTrue();
    }

    [Fact]
    public void UserMessage_None_ПустоБезFaq()
    {
        SteamReachabilityDiagnostics.UserMessage(SteamReachabilityFailureKind.None).Should().BeEmpty();
        SteamReachabilityDiagnostics.OfferFaq(SteamReachabilityFailureKind.None).Should().BeFalse();
    }

    [Fact]
    public void NotRunning_ПодсказываетЗапуститьИСеть()
    {
        var msg = SteamReachabilityDiagnostics.NotRunningUserMessage;
        msg.Should().Contain("Запустите Steam");
        msg.Should().Contain("VPN");
        msg.Should().Contain("firewall");
    }

    [Fact]
    public void InitFailed_ПодсказываетПерезапуск()
    {
        SteamReachabilityDiagnostics.InitFailedUserMessage.Should().Contain("перезапустите Steam");
    }
}

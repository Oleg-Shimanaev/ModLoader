using FluentAssertions;
using ModLoader.Core.Models;
using ModLoader.Core.Steam;
using Moq;

namespace ModLoader.Tests;

public class PublishServiceTests
{
    private readonly Mock<ISteamClient> _steam = new();
    private readonly PublishService _service;

    public PublishServiceTests()
    {
        _service = new PublishService(_steam.Object, new WorkshopVdfGenerator());
    }

    private static UploadParameters Params() =>
        UploadParameters.TryCreate(@"D:\mods\MyMod", 248610, "Title", "Desc", null).Value!;

    [Fact]
    public void EnsureSteamReady_SteamЗапущен_УспехБезЗапроса()
    {
        _steam.Setup(s => s.IsRunning()).Returns(true);

        var result = _service.EnsureSteamReady(allowLaunch: false);

        result.IsReady.Should().BeTrue();
        result.NeedsLaunchPermission.Should().BeFalse();
        _steam.Verify(s => s.LaunchSteam(), Times.Never);
    }

    [Fact]
    public void EnsureSteamReady_SteamНеЗапущен_НужноРазрешение()
    {
        _steam.Setup(s => s.IsRunning()).Returns(false);

        var result = _service.EnsureSteamReady(allowLaunch: false);

        result.IsReady.Should().BeFalse();
        result.NeedsLaunchPermission.Should().BeTrue();
        result.Message.Should().Contain("Steam");
    }

    [Fact]
    public void EnsureSteamReady_РазрешёнЗапуск_ЗапускаетSteam()
    {
        _steam.SetupSequence(s => s.IsRunning())
            .Returns(false)
            .Returns(true);
        _steam.Setup(s => s.LaunchSteam()).Returns(true);

        var result = _service.EnsureSteamReady(allowLaunch: true);

        result.IsReady.Should().BeTrue();
        _steam.Verify(s => s.LaunchSteam(), Times.Once);
    }

    [Fact]
    public void PublishViaGame_ВызываетЗапускИгрыСPublish()
    {
        _steam.Setup(s => s.IsRunning()).Returns(true);
        _steam.Setup(s => s.LaunchAppWithPublishArgs(248610, @"D:\mods\MyMod")).Returns(true);

        var result = _service.PublishViaGame(Params());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("запущен");
        _steam.Verify(s => s.LaunchAppWithPublishArgs(248610, @"D:\mods\MyMod"), Times.Once);
    }

    [Fact]
    public void PublishViaGame_SteamНеЗапущен_ОшибкаЧеловеческимЯзыком()
    {
        _steam.Setup(s => s.IsRunning()).Returns(false);

        var result = _service.PublishViaGame(Params());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Steam");
        _steam.Verify(s => s.LaunchAppWithPublishArgs(It.IsAny<uint>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PrepareSteamCmdUpload_ГенерируетVdfИКоманду()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns(@"C:\steamcmd\steamcmd.exe");

        var result = _service.PrepareSteamCmdUpload(Params());

        result.Success.Should().BeTrue();
        result.VdfPath.Should().NotBeNullOrEmpty();
        File.Exists(result.VdfPath!).Should().BeTrue();
        result.SuggestedCommand.Should().Contain("workshop_build_item");
        result.SuggestedCommand.Should().Contain(result.VdfPath);

        File.Delete(result.VdfPath!);
    }

    [Fact]
    public void PrepareSteamCmdUpload_SteamCmdНеНайден_ПонятнаяОшибка()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns((string?)null);

        var result = _service.PrepareSteamCmdUpload(Params());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("steamcmd");
    }
}

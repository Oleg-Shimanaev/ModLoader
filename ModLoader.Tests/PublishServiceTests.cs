using FluentAssertions;
using ModLoader.Core.Models;
using ModLoader.Core.Steam;
using ModLoader.Core.Wizard;
using Moq;

namespace ModLoader.Tests;

public class PublishServiceTests
{
    private readonly Mock<ISteamClient> _steam = new();
    private readonly Mock<IWorkshopPublisher> _workshop = new();
    private readonly PublishService _service;

    public PublishServiceTests()
    {
        _workshop.Setup(w => w.IsAvailable).Returns(true);
        _service = new PublishService(_steam.Object, new WorkshopVdfGenerator(), _workshop.Object);
    }

    private static UploadParameters Params(ulong publishedFileId = 0) =>
        UploadParameters.TryCreate(@"C:\mods\SampleMod", 248610, "Title", "Desc", null, publishedFileId).Value!;

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
    public async Task PublishAsync_SteamClient_НовыйМод_БезЛогинаИБезSteamCmd()
    {
        _steam.Setup(s => s.IsRunning()).Returns(true);
        _workshop
            .Setup(w => w.PublishAsync(It.IsAny<UploadParameters>(), It.IsAny<IProgress<float>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkshopPublishResult
            {
                Success = true,
                PublishedFileId = 1001,
                Message = "ok client"
            });

        var result = await _service.PublishAsync(Params(0), UploadMethod.SteamClient);

        result.Success.Should().BeTrue();
        result.PublishedFileId.Should().Be(1001);
        result.Message.Should().Be("ok client");
        _steam.Verify(s => s.LaunchSteamCmdWorkshopBuild(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _steam.Verify(s => s.LaunchAppWithPublishArgs(It.IsAny<uint>(), It.IsAny<string>()), Times.Never);
        _workshop.Verify(
            w => w.PublishAsync(It.IsAny<UploadParameters>(), It.IsAny<IProgress<float>?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_SteamClient_SteamНеЗапущен_Ошибка()
    {
        _steam.Setup(s => s.IsRunning()).Returns(false);

        var result = await _service.PublishAsync(Params(), UploadMethod.SteamClient);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Steam");
        _workshop.Verify(
            w => w.PublishAsync(It.IsAny<UploadParameters>(), It.IsAny<IProgress<float>?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishAsync_SteamCmd_НовыйМод_ИдётЧерезSteamCmd_НеЗапускаетИгру()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns(@"C:\steamcmd\steamcmd.exe");
        _steam.Setup(s => s.LaunchSteamCmdWorkshopBuild(It.IsAny<string>(), "player"))
            .Returns(true);

        var result = await _service.PublishAsync(Params(0), UploadMethod.SteamCmd, steamUsername: "player");

        result.Success.Should().BeTrue();
        result.VdfPath.Should().NotBeNullOrEmpty();
        File.Exists(result.VdfPath!).Should().BeTrue();
        File.ReadAllText(result.VdfPath!).Should().Contain("\"publishedfileid\"\t\t\"0\"");
        _steam.Verify(s => s.LaunchSteamCmdWorkshopBuild(result.VdfPath!, "player"), Times.Once);
        _steam.Verify(s => s.LaunchAppWithPublishArgs(It.IsAny<uint>(), It.IsAny<string>()), Times.Never);
        _workshop.Verify(
            w => w.PublishAsync(It.IsAny<UploadParameters>(), It.IsAny<IProgress<float>?>(), It.IsAny<CancellationToken>()),
            Times.Never);

        File.Delete(result.VdfPath!);
    }

    [Fact]
    public async Task PublishAsync_SteamCmd_Обновление_СPublishedFileId()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns(@"C:\steamcmd\steamcmd.exe");
        _steam.Setup(s => s.LaunchSteamCmdWorkshopBuild(It.IsAny<string>(), "player"))
            .Returns(true);

        var result = await _service.PublishAsync(
            Params(441616640),
            UploadMethod.SteamCmd,
            steamUsername: "player");

        result.Success.Should().BeTrue();
        File.ReadAllText(result.VdfPath!).Should().Contain("\"publishedfileid\"\t\t\"441616640\"");
        _steam.Verify(s => s.LaunchSteamCmdWorkshopBuild(result.VdfPath!, "player"), Times.Once);

        File.Delete(result.VdfPath!);
    }

    [Fact]
    public async Task PublishAsync_SteamCmd_БезЛогина_ПонятнаяОшибка()
    {
        var result = await _service.PublishAsync(Params(0), UploadMethod.SteamCmd, steamUsername: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("логин");
        _steam.Verify(s => s.LaunchSteamCmdWorkshopBuild(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PrepareSteamCmdUpload_ГенерируетVdfИКоманду()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns(@"C:\steamcmd\steamcmd.exe");

        var result = _service.PrepareSteamCmdUpload(Params(publishedFileId: 55), steamUsername: "user");

        result.Success.Should().BeTrue();
        result.VdfPath.Should().NotBeNullOrEmpty();
        File.Exists(result.VdfPath!).Should().BeTrue();
        result.SuggestedCommand.Should().Contain("workshop_build_item");
        result.SuggestedCommand.Should().Contain("+login user");
        result.SuggestedCommand.Should().Contain(result.VdfPath);
        var vdf = File.ReadAllText(result.VdfPath!);
        vdf.Should().Contain("\"publishedfileid\"\t\t\"55\"");
        vdf.Should().Contain("\"appid\"");
        vdf.Should().Contain("\"contentfolder\"");
        vdf.Should().Contain("\"visibility\"");
        vdf.Should().Contain("\"title\"");
        vdf.Should().Contain("\"description\"");
        vdf.Should().Contain("\"changenote\"");

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

    [Fact]
    public void GetSteamCmdUiStatus_Найден_СкрываетКнопкуУказать()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns(@"C:\steamcmd\steamcmd.exe");

        var ui = _service.GetSteamCmdUiStatus();

        ui.IsFound.Should().BeTrue();
        ui.ShowPickButton.Should().BeFalse();
        ui.Path.Should().Be(@"C:\steamcmd\steamcmd.exe");
        ui.StatusText.Should().Contain("steamcmd");
    }

    [Fact]
    public void GetSteamCmdUiStatus_НеНайден_ПоказываетКнопкуУказать()
    {
        _steam.Setup(s => s.FindSteamCmdPath()).Returns((string?)null);

        var ui = _service.GetSteamCmdUiStatus();

        ui.IsFound.Should().BeFalse();
        ui.ShowPickButton.Should().BeTrue();
        ui.StatusText.Should().Contain("не найден");
    }

    [Fact]
    public void LicenseHint_СодержитОжидаемыйТекст()
    {
        PublishMessages.NoLicenseHint.Should().Contain("нет лицензии");
        PublishMessages.NoLicenseHint.Should().Contain("не куплена");
        PublishMessages.NoLicenseHint.Should().Contain("AppID");
    }
}

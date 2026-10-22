using FluentAssertions;
using ModLoader.Core.Steam;
using Moq;

namespace ModLoader.Tests;

public class SteamUiStatusTests
{
    private readonly Mock<ISteamClient> _steam = new();
    private readonly PublishService _service;

    public SteamUiStatusTests()
    {
        _service = new PublishService(_steam.Object);
    }

    [Fact]
    public void GetSteamUiStatus_Запущен_СкрываетКнопкуЗапуска()
    {
        _steam.Setup(s => s.IsRunning()).Returns(true);

        var ui = _service.GetSteamUiStatus();

        ui.IsRunning.Should().BeTrue();
        ui.ShowLaunchButton.Should().BeFalse();
        ui.StatusText.Should().Contain("Steam");
    }

    [Fact]
    public void GetSteamUiStatus_НеЗапущен_ПоказываетКнопкуЗапуска()
    {
        _steam.Setup(s => s.IsRunning()).Returns(false);

        var ui = _service.GetSteamUiStatus();

        ui.IsRunning.Should().BeFalse();
        ui.ShowLaunchButton.Should().BeTrue();
        ui.StatusText.Should().Contain("не запущен");
    }
}


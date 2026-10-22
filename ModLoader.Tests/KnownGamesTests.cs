using FluentAssertions;
using ModLoader.Core;

namespace ModLoader.Tests;

public class KnownGamesTests
{
    [Fact]
    public void DoorKickers_AppId_248610()
    {
        KnownGames.All.Should().Contain(g => g.Name == "Door Kickers" && g.AppId == 248610);
    }

    [Fact]
    public void DoorKickers2_AppId_1239080()
    {
        // Store / Workshop: https://store.steampowered.com/app/1239080/
        KnownGames.All.Should().Contain(g => g.Name.Contains("Door Kickers 2") && g.AppId == 1239080);
        KnownGames.All.Should().NotContain(g => g.AppId == 1231430);
    }
}

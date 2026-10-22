using FluentAssertions;
using ModLoader.Core.Models;
using ModLoader.Core.Steam;
using Steamworks;
using Steamworks.Data;
using FpPublishResult = Steamworks.Ugc.PublishResult;

namespace ModLoader.Tests;

public class SteamworksWorkshopPublisherTests
{
    private static UploadParameters Params(ulong publishedFileId = 0) =>
        UploadParameters.TryCreate(
            Path.Combine(Path.GetTempPath(), "mod"),
            1239080,
            "Title",
            "Desc",
            null,
            publishedFileId).Value!;

    [Fact]
    public async Task PublishAsync_Успех_ВозвращаетPublishedFileIdБезПароля()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "ModLoaderSw_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDir);
        File.WriteAllBytes(Path.Combine(baseDir, "steam_api64.dll"), new byte[] { 1 });

        var shutdown = 0;
        var sut = new SteamworksWorkshopPublisher(
            appBaseDirectory: baseDir,
            init: (_, _) => true,
            shutdown: () => shutdown++,
            isLoggedOn: () => true,
            submit: (_, _, _) => Task.FromResult(new FpPublishResult
            {
                Result = Result.OK,
                FileId = 441616640,
                NeedsWorkshopAgreement = false
            }));

        try
        {
            var result = await sut.PublishAsync(Params());

            result.Success.Should().BeTrue();
            result.PublishedFileId.Should().Be(441616640);
            result.Message.Should().Contain("Steam-клиент");
            result.Message.Should().Contain("441616640");
            shutdown.Should().Be(1);
            File.Exists(Path.Combine(baseDir, "steam_appid.txt")).Should().BeTrue();
            File.ReadAllText(Path.Combine(baseDir, "steam_appid.txt")).Trim().Should().Be("1239080");
        }
        finally
        {
            Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public async Task PublishAsync_InitНеУдался_ПонятнаяОшибкаСFaq()
    {
        var sut = new SteamworksWorkshopPublisher(
            appBaseDirectory: Path.GetTempPath(),
            init: (_, _) => false,
            shutdown: () => { },
            isLoggedOn: () => false,
            submit: (_, _, _) => throw new InvalidOperationException("should not run"));

        var result = await sut.PublishAsync(Params());

        result.Success.Should().BeFalse();
        result.OfferFaq.Should().BeTrue();
        result.Message.Should().Contain("Steam");
    }

    [Fact]
    public async Task PublishAsync_НеВошёлВАккаунт_ПонятнаяОшибка()
    {
        var sut = new SteamworksWorkshopPublisher(
            appBaseDirectory: Path.GetTempPath(),
            init: (_, _) => true,
            shutdown: () => { },
            isLoggedOn: () => false,
            submit: (_, _, _) => throw new InvalidOperationException("should not run"));

        var result = await sut.PublishAsync(Params());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("вход");
    }

    [Fact]
    public void IsAvailable_ЕстьSteamApiDll_True()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "ModLoaderSwAvail_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDir);
        File.WriteAllBytes(Path.Combine(baseDir, "steam_api64.dll"), new byte[] { 1 });
        try
        {
            var sut = new SteamworksWorkshopPublisher(appBaseDirectory: baseDir);
            sut.IsAvailable.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(baseDir, true);
        }
    }
}

using FluentAssertions;
using ModLoader.Core.Models;

namespace ModLoader.Tests;

public class UploadParametersTests
{
    [Fact]
    public void Create_МинимальноВалидныеПараметры_Успех()
    {
        var result = UploadParameters.TryCreate(
            modFolder: @"C:\mods\SampleMod",
            appId: 248610,
            title: "Мой мод",
            description: "Описание",
            previewImagePath: null);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AppId.Should().Be(248610);
        result.Value.Title.Should().Be("Мой мод");
        result.Value.Description.Should().Be("Описание");
        result.Value.PreviewImagePath.Should().BeNull();
        result.Value.IsUpdate.Should().BeFalse();
    }

    [Fact]
    public void Create_СPublishedFileId_ЭтоОбновление()
    {
        var result = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 248610, "Title", "Desc", null, publishedFileId: 99);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsUpdate.Should().BeTrue();
        result.Value.PublishedFileId.Should().Be(99);
    }

    [Fact]
    public void Create_AppIdНоль_Ошибка()
    {
        var result = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 0, "Title", "Desc", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AppID");
    }

    [Fact]
    public void Create_ПустойЗаголовок_Ошибка()
    {
        var result = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 248610, "  ", "Desc", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("назван");
    }

    [Fact]
    public void Create_ПустоеОписание_Ошибка()
    {
        var result = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 248610, "Title", "", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("описан");
    }

    [Fact]
    public void Create_ПревьюНеСуществует_Ошибка()
    {
        var result = UploadParameters.TryCreate(
            @"C:\mods\SampleMod",
            248610,
            "Title",
            "Desc",
            @"C:\нет_такой_картинки_xyz.png");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("превью");
    }

    [Fact]
    public void Create_ПревьюСуществует_Успех()
    {
        var preview = Path.Combine(Path.GetTempPath(), "preview_" + Guid.NewGuid().ToString("N") + ".png");
        File.WriteAllBytes(preview, new byte[] { 1, 2, 3 });
        try
        {
            var result = UploadParameters.TryCreate(
                @"C:\mods\SampleMod", 248610, "Title", "Desc", preview);

            result.IsSuccess.Should().BeTrue();
            result.Value!.PreviewImagePath.Should().Be(preview);
        }
        finally
        {
            File.Delete(preview);
        }
    }
}

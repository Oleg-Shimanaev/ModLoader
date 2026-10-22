using FluentAssertions;
using ModLoader.Core.Models;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class WorkshopVdfGeneratorTests
{
    private readonly WorkshopVdfGenerator _generator = new();

    [Fact]
    public void Generate_НовыйМод_PublishedFileIdНоль_ВсеПоля()
    {
        var parameters = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 248610, "Мой мод", "Описание мода", null,
            publishedFileId: 0,
            changeNote: "Первая загрузка",
            visibility: 0).Value!;

        var vdf = _generator.Generate(parameters);

        vdf.Should().Contain("\"appid\"\t\t\"248610\"");
        vdf.Should().Contain("\"contentfolder\"");
        vdf.Should().Contain("SampleMod");
        vdf.Should().Contain("\"title\"\t\t\"Мой мод\"");
        vdf.Should().Contain("\"description\"\t\t\"Описание мода\"");
        vdf.Should().Contain("\"visibility\"\t\t\"0\"");
        vdf.Should().Contain("\"changenote\"\t\t\"Первая загрузка\"");
        vdf.Should().Contain("\"publishedfileid\"\t\t\"0\"");
        vdf.Should().NotContain("\"previewfile\"");
    }

    [Fact]
    public void Generate_Обновление_СPublishedFileIdИПревью()
    {
        var preview = Path.Combine(Path.GetTempPath(), "p_" + Guid.NewGuid().ToString("N") + ".jpg");
        File.WriteAllBytes(preview, new byte[] { 9 });
        try
        {
            var parameters = UploadParameters.TryCreate(
                @"C:\mods\SampleMod", 248610, "Title", "Desc", preview,
                publishedFileId: 12345,
                changeNote: "Патч",
                visibility: 2).Value!;

            var vdf = _generator.Generate(parameters);

            vdf.Should().Contain("\"previewfile\"");
            vdf.Should().Contain(Path.GetFileName(preview));
            vdf.Should().Contain("\"publishedfileid\"\t\t\"12345\"");
            vdf.Should().Contain("\"visibility\"\t\t\"2\"");
            vdf.Should().Contain("\"changenote\"\t\t\"Патч\"");
        }
        finally
        {
            File.Delete(preview);
        }
    }

    [Fact]
    public void WriteToTempFile_СоздаётФайлНаДиске()
    {
        var parameters = UploadParameters.TryCreate(
            @"C:\mods\SampleMod", 248610, "Title", "Desc", null).Value!;

        var path = _generator.WriteToTempFile(parameters);
        try
        {
            File.Exists(path).Should().BeTrue();
            File.ReadAllText(path).Should().Contain("248610");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

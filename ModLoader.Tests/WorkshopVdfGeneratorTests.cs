using FluentAssertions;
using ModLoader.Core.Models;
using ModLoader.Core.Steam;

namespace ModLoader.Tests;

public class WorkshopVdfGeneratorTests
{
    private readonly WorkshopVdfGenerator _generator = new();

    [Fact]
    public void Generate_СодержитAppIdИПутьКонтента()
    {
        var parameters = UploadParameters.TryCreate(
            @"D:\mods\MyMod", 248610, "Мой мод", "Описание мода", null).Value!;

        var vdf = _generator.Generate(parameters, publishedFileId: 0);

        vdf.Should().Contain("\"appid\"\t\t\"248610\"");
        vdf.Should().Contain("\"contentfolder\"");
        vdf.Should().Contain("MyMod");
        vdf.Should().Contain("\"title\"\t\t\"Мой мод\"");
        vdf.Should().Contain("\"description\"\t\t\"Описание мода\"");
        vdf.Should().Contain("\"publishedfileid\"\t\t\"0\"");

    }

    [Fact]
    public void Generate_СПревью_ДобавляетPreviewFile()
    {
        var preview = Path.Combine(Path.GetTempPath(), "p_" + Guid.NewGuid().ToString("N") + ".jpg");
        File.WriteAllBytes(preview, new byte[] { 9 });
        try
        {
            var parameters = UploadParameters.TryCreate(
                @"D:\mods\MyMod", 248610, "Title", "Desc", preview).Value!;

            var vdf = _generator.Generate(parameters, publishedFileId: 12345);

            vdf.Should().Contain("\"previewfile\"");
            vdf.Should().Contain(Path.GetFileName(preview));
            vdf.Should().Contain("\"publishedfileid\"\t\t\"12345\"");

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
            @"D:\mods\MyMod", 248610, "Title", "Desc", null).Value!;

        var path = _generator.WriteToTempFile(parameters, 0);
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

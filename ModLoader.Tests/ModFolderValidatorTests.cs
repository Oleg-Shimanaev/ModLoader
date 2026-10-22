using FluentAssertions;
using ModLoader.Core.Validation;

namespace ModLoader.Tests;

public class ModFolderValidatorTests
{
    private readonly ModFolderValidator _validator = new();

    [Fact]
    public void Validate_ПустойПуть_ВозвращаетОшибку()
    {
        var result = _validator.Validate("");

        result.IsValid.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("папк");
    }

    [Fact]
    public void Validate_ПапкаНеСуществует_ВозвращаетОшибку()
    {
        var result = _validator.Validate(@"C:\путь\которого\точно\нет_modloader_xyz");

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("не найден");
    }

    [Fact]
    public void Validate_ПустаяПапка_ВозвращаетОшибку()
    {
        var dir = CreateTempDir();
        try
        {
            var result = _validator.Validate(dir);

            result.IsValid.Should().BeFalse();
            result.Message.Should().Contain("пуст");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Validate_ПапкаСФайламиБезModXml_ВалиднаСПредупреждением()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "readme.txt"), "mod");

            var result = _validator.Validate(dir);

            result.IsValid.Should().BeTrue();
            result.HasWarning.Should().BeTrue();
            result.Message.Should().Contain("mod.xml");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Validate_ПапкаСModXml_ВалиднаБезПредупреждения()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "mod.xml"), "<mod name=\"Test\" />");

            var result = _validator.Validate(dir);

            result.IsValid.Should().BeTrue();
            result.HasWarning.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Validate_КореньMods_ОшибкаНужнаПодпапка()
    {
        var parent = CreateTempDir();
        var mods = Path.Combine(parent, "mods");
        Directory.CreateDirectory(mods);
        File.WriteAllText(Path.Combine(mods, "readme.txt"), "x");
        try
        {
            var result = _validator.Validate(mods);

            result.IsValid.Should().BeFalse();
            result.Message.Should().Contain("подпапк");
        }
        finally
        {
            Directory.Delete(parent, true);
        }
    }

    [Fact]
    public void Validate_КореньModsUpload_ОшибкаНужнаПодпапка()
    {
        var parent = CreateTempDir();
        var upload = Path.Combine(parent, "mods_upload");
        Directory.CreateDirectory(upload);
        File.WriteAllText(Path.Combine(upload, "readme.txt"), "x");
        try
        {
            var result = _validator.Validate(upload);

            result.IsValid.Should().BeFalse();
            result.Message.Should().Contain("mods_upload");
        }
        finally
        {
            Directory.Delete(parent, true);
        }
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ModLoaderTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}

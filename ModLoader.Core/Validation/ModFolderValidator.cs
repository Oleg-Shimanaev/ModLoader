namespace ModLoader.Core.Validation;

public sealed class ModFolderValidator
{
    public ValidationResult Validate(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return ValidationResult.Fail("Папка не выбрана.");

        if (!Directory.Exists(folderPath))
            return ValidationResult.Fail("Папка не найдена.");

        var entries = Directory.EnumerateFileSystemEntries(folderPath).Take(1).ToList();
        if (entries.Count == 0)
            return ValidationResult.Fail("Папка пуста.");

        // Корень mods / mods_upload — не сам мод (нужна подпапка с mod.xml).
        var name = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (name is "mods" or "mods_upload")
        {
            return ValidationResult.Fail(
                "Выбрана общая папка mods/mods_upload. Нужна подпапка одного мода с mod.xml. См. FAQ.");
        }

        var modXml = Path.Combine(folderPath, "mod.xml");
        if (!File.Exists(modXml))
        {
            return ValidationResult.Warning(
                "В папке нет mod.xml. Для Door Kickers 2 он обязателен для валидного мода. См. FAQ.");
        }

        return ValidationResult.Ok("Папка мода в порядке.");
    }
}

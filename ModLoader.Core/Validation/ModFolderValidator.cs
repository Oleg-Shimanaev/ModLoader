namespace ModLoader.Core.Validation;

public sealed class ModFolderValidator
{
    public ValidationResult Validate(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return ValidationResult.Fail("Выберите папку с модом.");

        if (!Directory.Exists(folderPath))
            return ValidationResult.Fail("Папка не найдена. Нажмите «Выбрать папку» ещё раз.");

        var entries = Directory.EnumerateFileSystemEntries(folderPath).Take(1).ToList();
        if (entries.Count == 0)
            return ValidationResult.Fail("Папка пуста. Положите туда файлы мода и попробуйте снова.");

        var modXml = Path.Combine(folderPath, "mod.xml");
        if (!File.Exists(modXml))
        {
            return ValidationResult.Warning(
                "В папке нет файла mod.xml. Для Door Kickers он обычно нужен. Можно продолжить, если игра это допускает.");
        }

        return ValidationResult.Ok("Папка с модом в порядке.");
    }
}

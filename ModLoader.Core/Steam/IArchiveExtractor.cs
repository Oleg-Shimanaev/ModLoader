namespace ModLoader.Core.Steam;

/// <summary>
/// Распаковка zip (для тестов мокается).
/// </summary>
public interface IArchiveExtractor
{
    void ExtractZip(string zipFilePath, string destinationDirectory);
}

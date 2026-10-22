namespace ModLoader.Core.Steam;

/// <summary>
/// Минимальные операции с файлами (для тестов мокается).
/// </summary>
public interface IFileSystem
{
    void EnsureDirectory(string path);
    bool FileExists(string path);
    void DeleteFileIfExists(string path);
}

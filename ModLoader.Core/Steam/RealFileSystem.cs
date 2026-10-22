namespace ModLoader.Core.Steam;

public sealed class RealFileSystem : IFileSystem
{
    public void EnsureDirectory(string path) =>
        Directory.CreateDirectory(path);

    public bool FileExists(string path) =>
        File.Exists(path);

    public void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}

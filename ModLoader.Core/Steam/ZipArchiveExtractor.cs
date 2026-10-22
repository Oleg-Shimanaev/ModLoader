using System.IO.Compression;

namespace ModLoader.Core.Steam;

public sealed class ZipArchiveExtractor : IArchiveExtractor
{
    public void ExtractZip(string zipFilePath, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory, overwriteFiles: true);
    }
}

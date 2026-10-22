namespace ModLoader.Core.Steam;

/// <summary>
/// Скачивание файла по HTTP (для тестов мокается).
/// </summary>
public interface IHttpDownloader
{
    Task DownloadFileAsync(Uri url, string destinationFile, CancellationToken cancellationToken = default);
}

namespace ModLoader.Core.Steam;

public sealed class HttpClientDownloader : IHttpDownloader
{
    private readonly HttpClient _client;

    public HttpClientDownloader(HttpClient? client = null)
    {
        _client = client ?? new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task DownloadFileAsync(
        Uri url,
        string destinationFile,
        CancellationToken cancellationToken = default)
    {
        await using var response = await _client
            .GetStreamAsync(url, cancellationToken)
            .ConfigureAwait(false);
        await using var file = File.Create(destinationFile);
        await response.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
    }
}

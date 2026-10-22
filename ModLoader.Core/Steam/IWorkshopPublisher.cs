using ModLoader.Core.Models;

namespace ModLoader.Core.Steam;

public sealed class WorkshopPublishResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public ulong PublishedFileId { get; init; }
    public bool OfferFaq { get; init; }
}

/// <summary>
/// Публикация в Workshop через Steamworks (сессия запущенного Steam-клиента).
/// </summary>
public interface IWorkshopPublisher
{
    /// <summary>
    /// Можно ли инициализировать Steamworks (есть native DLL и т.п.).
    /// </summary>
    bool IsAvailable { get; }

    Task<WorkshopPublishResult> PublishAsync(
        UploadParameters parameters,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default);
}

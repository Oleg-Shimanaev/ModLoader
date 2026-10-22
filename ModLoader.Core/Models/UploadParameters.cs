namespace ModLoader.Core.Models;

/// <summary>
/// Параметры загрузки мода в Steam Workshop.
/// </summary>
public sealed class UploadParameters
{
    public string ModFolder { get; }
    public uint AppId { get; }
    public string Title { get; }
    public string Description { get; }
    public string? PreviewImagePath { get; }

    /// <summary>
    /// 0 — новый мод; иначе Published File ID для обновления существующего item.
    /// </summary>
    public ulong PublishedFileId { get; }

    /// <summary>
    /// 0 = public, 1 = friends only, 2 = private.
    /// </summary>
    public int Visibility { get; }

    public string ChangeNote { get; }

    public bool IsUpdate => PublishedFileId > 0;

    private UploadParameters(
        string modFolder,
        uint appId,
        string title,
        string description,
        string? previewImagePath,
        ulong publishedFileId,
        int visibility,
        string changeNote)
    {
        ModFolder = modFolder;
        AppId = appId;
        Title = title;
        Description = description;
        PreviewImagePath = previewImagePath;
        PublishedFileId = publishedFileId;
        Visibility = visibility;
        ChangeNote = changeNote;
    }

    public static OperationResult<UploadParameters> TryCreate(
        string modFolder,
        uint appId,
        string title,
        string description,
        string? previewImagePath,
        ulong publishedFileId = 0,
        string? changeNote = null,
        int visibility = 0)
    {
        if (string.IsNullOrWhiteSpace(modFolder))
            return OperationResult<UploadParameters>.Fail(
                "Папка с модом не выбрана. См. FAQ.");

        if (appId == 0)
            return OperationResult<UploadParameters>.Fail(
                "Не выбран AppID игры. См. FAQ.");

        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<UploadParameters>.Fail(
                "Нет названия мода.");

        if (string.IsNullOrWhiteSpace(description))
            return OperationResult<UploadParameters>.Fail(
                "Нет описания мода.");

        if (!string.IsNullOrWhiteSpace(previewImagePath) && !File.Exists(previewImagePath))
            return OperationResult<UploadParameters>.Fail(
                "Файл превью не найден.");

        if (visibility is < 0 or > 2)
            return OperationResult<UploadParameters>.Fail(
                "Visibility: 0 (public), 1 (friends), 2 (private).");

        var note = string.IsNullOrWhiteSpace(changeNote)
            ? "Uploaded via ModLoader"
            : changeNote.Trim();

        return OperationResult<UploadParameters>.Ok(new UploadParameters(
            modFolder.Trim(),
            appId,
            title.Trim(),
            description.Trim(),
            string.IsNullOrWhiteSpace(previewImagePath) ? null : previewImagePath.Trim(),
            publishedFileId,
            visibility,
            note));
    }
}

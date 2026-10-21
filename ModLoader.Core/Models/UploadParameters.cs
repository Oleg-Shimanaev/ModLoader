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

    private UploadParameters(
        string modFolder,
        uint appId,
        string title,
        string description,
        string? previewImagePath)
    {
        ModFolder = modFolder;
        AppId = appId;
        Title = title;
        Description = description;
        PreviewImagePath = previewImagePath;
    }

    public static OperationResult<UploadParameters> TryCreate(
        string modFolder,
        uint appId,
        string title,
        string description,
        string? previewImagePath)
    {
        if (string.IsNullOrWhiteSpace(modFolder))
            return OperationResult<UploadParameters>.Fail("Укажите папку с модом.");

        if (appId == 0)
            return OperationResult<UploadParameters>.Fail("Укажите AppID игры (число больше нуля).");

        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<UploadParameters>.Fail("Укажите название мода.");

        if (string.IsNullOrWhiteSpace(description))
            return OperationResult<UploadParameters>.Fail("Укажите описание мода.");

        if (!string.IsNullOrWhiteSpace(previewImagePath) && !File.Exists(previewImagePath))
            return OperationResult<UploadParameters>.Fail("Файл превью не найден. Выберите картинку заново.");

        return OperationResult<UploadParameters>.Ok(new UploadParameters(
            modFolder.Trim(),
            appId,
            title.Trim(),
            description.Trim(),
            string.IsNullOrWhiteSpace(previewImagePath) ? null : previewImagePath.Trim()));
    }
}

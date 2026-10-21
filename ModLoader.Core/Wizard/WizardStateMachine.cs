using ModLoader.Core.Models;
using ModLoader.Core.Validation;

namespace ModLoader.Core.Wizard;

/// <summary>
/// Пошаговый мастер: папка → игра → название → превью → подтверждение.
/// </summary>
public sealed class WizardStateMachine
{
    private readonly ModFolderValidator _validator = new();

    public WizardStep CurrentStep { get; private set; } = WizardStep.ModFolder;

    public string? ModFolder { get; private set; }
    public uint AppId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? PreviewImagePath { get; private set; }
    public string? LastMessage { get; private set; }

    public bool CanGoBack => CurrentStep > WizardStep.ModFolder;
    public bool CanGoNext => CurrentStep != WizardStep.Confirm && IsCurrentStepComplete();

    public bool SetModFolder(string folderPath)
    {
        var result = _validator.Validate(folderPath);
        LastMessage = result.Message;
        if (!result.IsValid)
        {
            ModFolder = null;
            return false;
        }

        ModFolder = folderPath;
        return true;
    }

    public void SetAppId(uint appId)
    {
        AppId = appId;
        LastMessage = appId == 0
            ? "Введите AppID игры (его можно найти в свойствах игры в Steam)."
            : $"Выбрана игра с AppID {appId}.";
    }

    public void SetTitleAndDescription(string title, string description)
    {
        Title = title?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        LastMessage = IsTitleDescriptionComplete()
            ? "Название и описание сохранены."
            : "Заполните название и описание.";
    }

    public void SetPreviewImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            PreviewImagePath = null;
            LastMessage = "Превью не выбрано — это нормально, можно продолжить.";
            return;
        }

        if (!File.Exists(path))
        {
            LastMessage = "Картинка не найдена. Выберите файл ещё раз или пропустите шаг.";
            PreviewImagePath = null;
            return;
        }

        PreviewImagePath = path;
        LastMessage = "Превью выбрано.";
    }

    public void Next()
    {
        if (CurrentStep == WizardStep.Confirm)
            throw new InvalidOperationException("Это последний шаг. Нажмите «Загрузить».");

        if (!IsCurrentStepComplete())
            throw new InvalidOperationException("Сначала заполните этот шаг.");

        CurrentStep++;
    }

    public void Back()
    {
        if (!CanGoBack)
            return;

        CurrentStep--;
    }

    public OperationResult<UploadParameters> BuildUploadParameters()
    {
        if (ModFolder is null)
            return OperationResult<UploadParameters>.Fail("Папка мода не выбрана.");

        return UploadParameters.TryCreate(ModFolder, AppId, Title, Description, PreviewImagePath);
    }

    private bool IsCurrentStepComplete() => CurrentStep switch
    {
        WizardStep.ModFolder => !string.IsNullOrWhiteSpace(ModFolder),
        WizardStep.GameAppId => AppId > 0,
        WizardStep.TitleDescription => IsTitleDescriptionComplete(),
        WizardStep.Preview => true,
        WizardStep.Confirm => true,
        _ => false
    };

    private bool IsTitleDescriptionComplete() =>
        !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Description);
}

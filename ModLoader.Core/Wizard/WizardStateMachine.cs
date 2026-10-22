using ModLoader.Core.Models;
using ModLoader.Core.Steam;
using ModLoader.Core.Validation;

namespace ModLoader.Core.Wizard;

/// <summary>
/// Пошаговый мастер: папка → игра → новый/обновить → название → превью → способ → подтверждение.
/// </summary>
public sealed class WizardStateMachine
{
    private readonly ModFolderValidator _validator = new();

    public WizardStep CurrentStep { get; private set; } = WizardStep.ModFolder;

    public string? ModFolder { get; private set; }
    public uint AppId { get; private set; }
    public PublishKind PublishKind { get; private set; } = PublishKind.CreateNew;
    public ulong PublishedFileId { get; private set; }
    public string WorkshopIdInput { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? PreviewImagePath { get; private set; }
    public UploadMethod UploadMethod { get; private set; } = UploadMethod.SteamClient;
    public string? LastMessage { get; private set; }

    public int StepNumber => (int)CurrentStep + 1;
    public int TotalSteps => 7;

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
        // KillHouse templates: mod_image.jpg рядом с mod.xml
        if (string.IsNullOrWhiteSpace(PreviewImagePath))
        {
            var preview = GameModsPathFinder.FindDefaultPreviewImage(folderPath);
            if (preview is not null)
                PreviewImagePath = preview;
        }

        return true;
    }

    public void SetAppId(uint appId)
    {
        AppId = appId;
        LastMessage = appId == 0 ? "Выберите игру или AppID." : $"AppID {appId}.";
    }

    public void SetPublishKindCreateNew()
    {
        PublishKind = PublishKind.CreateNew;
        PublishedFileId = 0;
        WorkshopIdInput = string.Empty;
        LastMessage = "Новый мод.";
    }

    public void SetPublishKindUpdate()
    {
        PublishKind = PublishKind.UpdateExisting;
        LastMessage = "Обновление — укажите Workshop ID.";
    }

    public bool SetWorkshopIdInput(string? input)
    {
        WorkshopIdInput = input?.Trim() ?? string.Empty;
        if (PublishKind != PublishKind.UpdateExisting)
        {
            PublishedFileId = 0;
            return true;
        }

        if (!WorkshopIdParser.TryParse(WorkshopIdInput, out var id))
        {
            PublishedFileId = 0;
            LastMessage = "Нужен Workshop ID. См. FAQ.";
            return false;
        }

        PublishedFileId = id;
        LastMessage = $"Workshop ID {id}.";
        return true;
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
            LastMessage = "Превью пропущено.";
            return;
        }

        if (!File.Exists(path))
        {
            LastMessage = "Картинка не найдена.";
            PreviewImagePath = null;
            return;
        }

        PreviewImagePath = path;
        LastMessage = "Превью выбрано.";
    }

    public void SetUploadMethodSteamClient()
    {
        UploadMethod = UploadMethod.SteamClient;
        LastMessage = "Через Steam-клиент (без пароля).";
    }

    public void SetUploadMethodSteamCmd()
    {
        UploadMethod = UploadMethod.SteamCmd;
        LastMessage = "Через steamcmd (логин / пароль в окне steamcmd).";
    }

    public void Next()
    {
        if (CurrentStep == WizardStep.Confirm)
            throw new InvalidOperationException("Последний шаг — нажмите «Загрузить».");

        if (!IsCurrentStepComplete())
            throw new InvalidOperationException(GetIncompleteStepHint());

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

        if (PublishKind == PublishKind.UpdateExisting && PublishedFileId == 0)
            return OperationResult<UploadParameters>.Fail("Не указан Workshop ID. См. FAQ.");

        return UploadParameters.TryCreate(
            ModFolder,
            AppId,
            Title,
            Description,
            PreviewImagePath,
            PublishKind == PublishKind.UpdateExisting ? PublishedFileId : 0);
    }

    private string GetIncompleteStepHint() => CurrentStep switch
    {
        WizardStep.ModFolder => "Выберите папку с модом.",
        WizardStep.GameAppId => "Выберите игру или AppID.",
        WizardStep.PublishKind => PublishKind == PublishKind.UpdateExisting
            ? "Укажите Workshop ID или выберите «Новый мод»."
            : "Выберите «Новый мод» или «Обновить».",
        WizardStep.TitleDescription => "Заполните название и описание.",
        WizardStep.UploadMethod => "Выберите способ загрузки.",
        _ => "Заполните этот шаг."
    };

    private bool IsCurrentStepComplete() => CurrentStep switch
    {
        WizardStep.ModFolder => !string.IsNullOrWhiteSpace(ModFolder),
        WizardStep.GameAppId => AppId > 0,
        WizardStep.PublishKind => PublishKind == PublishKind.CreateNew || PublishedFileId > 0,
        WizardStep.TitleDescription => IsTitleDescriptionComplete(),
        WizardStep.Preview => true,
        WizardStep.UploadMethod => true,
        WizardStep.Confirm => true,
        _ => false
    };

    private bool IsTitleDescriptionComplete() =>
        !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Description);
}

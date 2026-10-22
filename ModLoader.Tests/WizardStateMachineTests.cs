using FluentAssertions;
using ModLoader.Core.Wizard;

namespace ModLoader.Tests;

public class WizardStateMachineTests
{
    [Fact]
    public void Старт_НаШагеВыбораПапки()
    {
        var wizard = new WizardStateMachine();

        wizard.CurrentStep.Should().Be(WizardStep.ModFolder);
        wizard.CanGoBack.Should().BeFalse();
        wizard.CanGoNext.Should().BeFalse();
        wizard.TotalSteps.Should().Be(7);
        wizard.UploadMethod.Should().Be(UploadMethod.SteamClient);
    }

    [Fact]
    public void SetModFolder_ВалиднаяПапка_МожноДалее()
    {
        var wizard = new WizardStateMachine();
        var dir = CreateModDir(withXml: true);
        try
        {
            var ok = wizard.SetModFolder(dir);

            ok.Should().BeTrue();
            wizard.CanGoNext.Should().BeTrue();
            wizard.ModFolder.Should().Be(dir);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void SetModFolder_АвтоподставляетModImageJpg()
    {
        var wizard = new WizardStateMachine();
        var dir = CreateModDir(withXml: true);
        var preview = Path.Combine(dir, "mod_image.jpg");
        File.WriteAllBytes(preview, new byte[] { 1, 2 });
        try
        {
            wizard.SetModFolder(dir).Should().BeTrue();
            wizard.PreviewImagePath.Should().Be(preview);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Next_ПослеПапки_ПереходитКИгре()
    {
        var wizard = ReadyAt(WizardStep.ModFolder);

        wizard.Next();

        wizard.CurrentStep.Should().Be(WizardStep.GameAppId);
    }

    [Fact]
    public void SetAppId_Невалидный_НельзяДалее()
    {
        var wizard = ReadyAt(WizardStep.GameAppId);

        wizard.SetAppId(0);

        wizard.CanGoNext.Should().BeFalse();
    }

    [Fact]
    public void SetAppId_Валидный_МожноДалее()
    {
        var wizard = ReadyAt(WizardStep.GameAppId);

        wizard.SetAppId(248610);

        wizard.CanGoNext.Should().BeTrue();
        wizard.AppId.Should().Be(248610);
    }

    [Fact]
    public void Next_ПослеAppId_ПереходитКРежимуПубликации()
    {
        var wizard = ReadyAt(WizardStep.GameAppId);
        wizard.SetAppId(248610);

        wizard.Next();

        wizard.CurrentStep.Should().Be(WizardStep.PublishKind);
    }

    [Fact]
    public void PublishKind_НовыйМод_МожноДалееБезId()
    {
        var wizard = ReadyAt(WizardStep.PublishKind);

        wizard.SetPublishKindCreateNew();

        wizard.CanGoNext.Should().BeTrue();
        wizard.PublishedFileId.Should().Be(0);
    }

    [Fact]
    public void PublishKind_ОбновитьБезId_НельзяДалее()
    {
        var wizard = ReadyAt(WizardStep.PublishKind);

        wizard.SetPublishKindUpdate();

        wizard.CanGoNext.Should().BeFalse();
    }

    [Fact]
    public void PublishKind_ОбновитьСIdИзСсылки_МожноДалее()
    {
        var wizard = ReadyAt(WizardStep.PublishKind);
        wizard.SetPublishKindUpdate();

        var ok = wizard.SetWorkshopIdInput("https://steamcommunity.com/sharedfiles/filedetails/?id=441616640");

        ok.Should().BeTrue();
        wizard.CanGoNext.Should().BeTrue();
        wizard.PublishedFileId.Should().Be(441616640);
    }

    [Fact]
    public void Next_ПослеPublishKind_ПереходитКНазванию()
    {
        var wizard = ReadyAt(WizardStep.PublishKind);
        wizard.SetPublishKindCreateNew();

        wizard.Next();

        wizard.CurrentStep.Should().Be(WizardStep.TitleDescription);
        wizard.CanGoNext.Should().BeFalse();
    }

    [Fact]
    public void SetTitleAndDescription_Заполнены_МожноДалее()
    {
        var wizard = ReadyAt(WizardStep.TitleDescription);

        wizard.SetTitleAndDescription("Мод", "Описание");

        wizard.CanGoNext.Should().BeTrue();
    }

    [Fact]
    public void Preview_Опционален_МожноПропустить()
    {
        var wizard = ReadyAt(WizardStep.Preview);

        wizard.CanGoNext.Should().BeTrue();

        wizard.Next();

        wizard.CurrentStep.Should().Be(WizardStep.UploadMethod);
    }

    [Fact]
    public void UploadMethod_ПоУмолчаниюSteamClient_МожноДалее()
    {
        var wizard = ReadyAt(WizardStep.UploadMethod);

        wizard.UploadMethod.Should().Be(UploadMethod.SteamClient);
        wizard.CanGoNext.Should().BeTrue();

        wizard.SetUploadMethodSteamCmd();
        wizard.UploadMethod.Should().Be(UploadMethod.SteamCmd);

        wizard.Next();
        wizard.CurrentStep.Should().Be(WizardStep.Confirm);
    }

    [Fact]
    public void Back_ВозвращаетНаПредыдущийШаг()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        wizard.Back();
        wizard.CurrentStep.Should().Be(WizardStep.UploadMethod);

        wizard.Back();
        wizard.CurrentStep.Should().Be(WizardStep.Preview);

        wizard.Back();
        wizard.CurrentStep.Should().Be(WizardStep.TitleDescription);
    }

    [Fact]
    public void Confirm_СобираетПараметрыЗагрузки_НовыйМод()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        var parameters = wizard.BuildUploadParameters();

        parameters.IsSuccess.Should().BeTrue();
        parameters.Value!.AppId.Should().Be(248610);
        parameters.Value.Title.Should().Be("Мод");
        parameters.Value.IsUpdate.Should().BeFalse();
        parameters.Value.PublishedFileId.Should().Be(0);
    }

    [Fact]
    public void Confirm_СобираетПараметры_Обновление()
    {
        var wizard = ReadyAt(WizardStep.Confirm, updateId: 441616640);

        var parameters = wizard.BuildUploadParameters();

        parameters.IsSuccess.Should().BeTrue();
        parameters.Value!.IsUpdate.Should().BeTrue();
        parameters.Value.PublishedFileId.Should().Be(441616640);
    }

    [Fact]
    public void Next_НаConfirm_Нельзя()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        var act = () => wizard.Next();

        act.Should().Throw<InvalidOperationException>();
    }

    private static WizardStateMachine ReadyAt(WizardStep target, ulong updateId = 0)
    {
        var dir = CreateModDir(withXml: true);
        var wizard = new WizardStateMachine();
        wizard.SetModFolder(dir);

        if (target == WizardStep.ModFolder)
            return wizard;

        wizard.Next();
        wizard.SetAppId(248610);
        if (target == WizardStep.GameAppId)
            return wizard;

        wizard.Next();
        if (updateId > 0)
        {
            wizard.SetPublishKindUpdate();
            wizard.SetWorkshopIdInput(updateId.ToString());
        }
        else
        {
            wizard.SetPublishKindCreateNew();
        }

        if (target == WizardStep.PublishKind)
            return wizard;

        wizard.Next();
        wizard.SetTitleAndDescription("Мод", "Описание");
        if (target == WizardStep.TitleDescription)
            return wizard;

        wizard.Next();
        if (target == WizardStep.Preview)
            return wizard;

        wizard.Next();
        if (target == WizardStep.UploadMethod)
            return wizard;

        wizard.Next();
        return wizard;
    }

    private static string CreateModDir(bool withXml)
    {
        var dir = Path.Combine(Path.GetTempPath(), "ModLoaderWiz_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, withXml ? "mod.xml" : "file.txt"), "<mod/>");
        return dir;
    }
}

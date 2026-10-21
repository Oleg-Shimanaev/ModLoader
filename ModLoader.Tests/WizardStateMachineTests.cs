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
    public void Next_ПослеAppId_ПереходитКНазванию()
    {
        var wizard = ReadyAt(WizardStep.GameAppId);
        wizard.SetAppId(248610);

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

        wizard.CurrentStep.Should().Be(WizardStep.Confirm);
    }

    [Fact]
    public void Back_ВозвращаетНаПредыдущийШаг()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        wizard.Back();
        wizard.CurrentStep.Should().Be(WizardStep.Preview);

        wizard.Back();
        wizard.CurrentStep.Should().Be(WizardStep.TitleDescription);
    }

    [Fact]
    public void Confirm_СобираетПараметрыЗагрузки()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        var parameters = wizard.BuildUploadParameters();

        parameters.IsSuccess.Should().BeTrue();
        parameters.Value!.AppId.Should().Be(248610);
        parameters.Value.Title.Should().Be("Мод");
    }

    [Fact]
    public void Next_НаConfirm_Нельзя()
    {
        var wizard = ReadyAt(WizardStep.Confirm);

        var act = () => wizard.Next();

        act.Should().Throw<InvalidOperationException>();
    }

    private static WizardStateMachine ReadyAt(WizardStep target)
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
        wizard.SetTitleAndDescription("Мод", "Описание");
        if (target == WizardStep.TitleDescription)
            return wizard;

        wizard.Next();
        if (target == WizardStep.Preview)
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

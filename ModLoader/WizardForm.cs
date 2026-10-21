using ModLoader.Core;
using ModLoader.Core.Steam;
using ModLoader.Core.Wizard;

namespace ModLoader;

/// <summary>
/// Тонкий UI: кнопки, выбор папок/файлов и простые подтверждения.
/// Логика — в WizardStateMachine и PublishService.
/// </summary>
public sealed class WizardForm : Form
{
    private readonly WizardStateMachine _wizard = new();
    private readonly PublishService _publish;
    private readonly ISteamClient _steam;

    private readonly Label _stepTitle = new();
    private readonly Label _hint = new();
    private readonly Label _status = new();
    private readonly Panel _content = new();
    private readonly Button _btnBack = new();
    private readonly Button _btnNext = new();
    private readonly Button _btnUpload = new();
    private readonly Button _btnLaunchSteam = new();

    // Контролы шагов
    private readonly Button _btnPickFolder = new();
    private readonly Label _folderLabel = new();
    private readonly FlowLayoutPanel _gamesPanel = new();
    private readonly NumericUpDown _appIdInput = new();
    private readonly TextBox _titleBox = new();
    private readonly TextBox _descriptionBox = new();
    private readonly Button _btnPickPreview = new();
    private readonly Button _btnSkipPreview = new();
    private readonly Label _previewLabel = new();
    private readonly Label _summaryLabel = new();

    public WizardForm(ISteamClient steam)
    {
        _steam = steam;
        _publish = new PublishService(steam);

        Text = "ModLoader — загрузка мода в Steam";
        Width = 640;
        Height = 480;
        MinimumSize = new Size(560, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        BuildChrome();
        BuildStepControls();
        WireEvents();
        Render();
    }

    private void BuildChrome()
    {
        _stepTitle.AutoSize = false;
        _stepTitle.Dock = DockStyle.Top;
        _stepTitle.Height = 36;
        _stepTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        _stepTitle.Padding = new Padding(12, 8, 12, 0);

        _hint.AutoSize = false;
        _hint.Dock = DockStyle.Top;
        _hint.Height = 48;
        _hint.Padding = new Padding(12, 4, 12, 4);

        _content.Dock = DockStyle.Fill;
        _content.Padding = new Padding(12);

        _status.AutoSize = false;
        _status.Dock = DockStyle.Bottom;
        _status.Height = 56;
        _status.Padding = new Padding(12, 4, 12, 4);
        _status.ForeColor = Color.DimGray;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };

        StyleButton(_btnUpload, "Загрузить", true);
        StyleButton(_btnNext, "Далее →", true);
        StyleButton(_btnBack, "← Назад", false);
        StyleButton(_btnLaunchSteam, "Запустить Steam", false);

        buttons.Controls.Add(_btnUpload);
        buttons.Controls.Add(_btnNext);
        buttons.Controls.Add(_btnBack);
        buttons.Controls.Add(_btnLaunchSteam);

        Controls.Add(_content);
        Controls.Add(buttons);
        Controls.Add(_status);
        Controls.Add(_hint);
        Controls.Add(_stepTitle);
    }

    private static void StyleButton(Button button, string text, bool primary)
    {
        button.Text = text;
        button.AutoSize = true;
        button.Padding = new Padding(12, 6, 12, 6);
        button.Margin = new Padding(6, 4, 6, 4);
        if (primary)
        {
            button.BackColor = Color.FromArgb(24, 104, 180);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
        }
    }

    private void BuildStepControls()
    {
        StyleButton(_btnPickFolder, "Выбрать папку мода…", true);
        _folderLabel.AutoSize = true;
        _folderLabel.MaximumSize = new Size(560, 0);

        _gamesPanel.FlowDirection = FlowDirection.TopDown;
        _gamesPanel.AutoSize = true;
        _gamesPanel.WrapContents = false;

        foreach (var (name, appId) in KnownGames.All)
        {
            var id = appId;
            var btn = new Button
            {
                Text = $"{name}  (AppID {appId})",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            btn.Click += (_, _) =>
            {
                _wizard.SetAppId(id);
                _appIdInput.Value = id;
                Render();
            };
            _gamesPanel.Controls.Add(btn);
        }

        var otherLabel = new Label
        {
            Text = "Или введите AppID другой игры:",
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 4)
        };
        _appIdInput.Maximum = uint.MaxValue;
        _appIdInput.Minimum = 0;
        _appIdInput.Width = 160;
        _appIdInput.ValueChanged += (_, _) =>
        {
            _wizard.SetAppId((uint)_appIdInput.Value);
            Render();
        };
        _gamesPanel.Controls.Add(otherLabel);
        _gamesPanel.Controls.Add(_appIdInput);

        _titleBox.PlaceholderText = "Название мода";
        _titleBox.Width = 480;
        _titleBox.TextChanged += (_, _) =>
        {
            _wizard.SetTitleAndDescription(_titleBox.Text, _descriptionBox.Text);
            RenderNavOnly();
        };

        _descriptionBox.PlaceholderText = "Короткое описание для Workshop";
        _descriptionBox.Multiline = true;
        _descriptionBox.Height = 100;
        _descriptionBox.Width = 480;
        _descriptionBox.TextChanged += (_, _) =>
        {
            _wizard.SetTitleAndDescription(_titleBox.Text, _descriptionBox.Text);
            RenderNavOnly();
        };

        StyleButton(_btnPickPreview, "Выбрать картинку…", true);
        StyleButton(_btnSkipPreview, "Пропустить", false);
        _previewLabel.AutoSize = true;

        _summaryLabel.AutoSize = true;
        _summaryLabel.MaximumSize = new Size(560, 0);
    }

    private void WireEvents()
    {
        _btnBack.Click += (_, _) =>
        {
            _wizard.Back();
            Render();
        };

        _btnNext.Click += (_, _) =>
        {
            try
            {
                _wizard.Next();
                Render();
            }
            catch (InvalidOperationException ex)
            {
                ShowHumanError(ex.Message);
            }
        };

        _btnPickFolder.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку с модом",
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            if (!_wizard.SetModFolder(dialog.SelectedPath))
            {
                ShowHumanError(_wizard.LastMessage ?? "Не удалось выбрать папку.");
                Render();
                return;
            }

            if (_wizard.LastMessage is not null && _wizard.LastMessage.Contains("mod.xml", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, _wizard.LastMessage, "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Render();
        };

        _btnPickPreview.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Картинка превью для Workshop",
                Filter = "Картинки|*.png;*.jpg;*.jpeg;*.gif|Все файлы|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _wizard.SetPreviewImage(dialog.FileName);
            Render();
        };

        _btnSkipPreview.Click += (_, _) =>
        {
            _wizard.SetPreviewImage(null);
            try
            {
                _wizard.Next();
                Render();
            }
            catch (InvalidOperationException ex)
            {
                ShowHumanError(ex.Message);
            }
        };

        _btnLaunchSteam.Click += (_, _) => TryLaunchSteam();

        _btnUpload.Click += (_, _) => DoUpload();
    }

    private void TryLaunchSteam()
    {
        var confirm = MessageBox.Show(
            this,
            "Разрешить запуск Steam?",
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        var result = _publish.EnsureSteamReady(allowLaunch: true);
        _status.Text = result.Message;
        if (!result.IsReady && !result.Message.Contains("запускается", StringComparison.OrdinalIgnoreCase))
            ShowHumanError(result.Message);
    }

    private void DoUpload()
    {
        var parameters = _wizard.BuildUploadParameters();
        if (!parameters.IsSuccess)
        {
            ShowHumanError(parameters.Error ?? "Проверьте заполненные поля.");
            return;
        }

        if (!_steam.IsRunning())
        {
            var ask = MessageBox.Show(
                this,
                "Steam не запущен.\n\nРазрешить запуск Steam и продолжить?",
                "Нужен Steam",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ask != DialogResult.Yes)
            {
                _status.Text = "Нажмите «Запустить Steam» или откройте Steam сами, затем снова «Загрузить».";
                return;
            }

            var ready = _publish.EnsureSteamReady(allowLaunch: true);
            _status.Text = ready.Message;
            if (!_steam.IsRunning())
            {
                MessageBox.Show(
                    this,
                    "Подождите, пока Steam откроется и вы войдёте в аккаунт, затем нажмите «Загрузить» ещё раз.",
                    "Почти готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
        }

        var go = MessageBox.Show(
            this,
            "Сейчас будет запущена игра через Steam в режиме публикации мода.\n\nПродолжить?",
            "Загрузка",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (go != DialogResult.Yes)
            return;

        var result = _publish.PublishViaGame(parameters.Value!);
        _status.Text = result.Message;
        MessageBox.Show(
            this,
            result.Message,
            result.Success ? "Готово" : "Не получилось",
            MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void Render()
    {
        _content.Controls.Clear();

        switch (_wizard.CurrentStep)
        {
            case WizardStep.ModFolder:
                _stepTitle.Text = "Шаг 1 из 5 — Папка мода";
                _hint.Text = "Выберите папку, в которой лежат файлы мода (для Door Kickers там обычно есть mod.xml).";
                _content.Controls.Add(Wrap(_btnPickFolder, _folderLabel));
                _folderLabel.Text = string.IsNullOrEmpty(_wizard.ModFolder)
                    ? "Папка ещё не выбрана."
                    : $"Выбрано: {_wizard.ModFolder}";
                break;

            case WizardStep.GameAppId:
                _stepTitle.Text = "Шаг 2 из 5 — Игра";
                _hint.Text = "Нажмите кнопку с игрой или введите AppID вручную (число из магазина Steam).";
                _content.Controls.Add(_gamesPanel);
                if (_wizard.AppId > 0)
                    _appIdInput.Value = _wizard.AppId;
                break;

            case WizardStep.TitleDescription:
                _stepTitle.Text = "Шаг 3 из 5 — Название и описание";
                _hint.Text = "Как мод будет называться в Steam Workshop. Коротко и понятно.";
                _titleBox.Text = _wizard.Title;
                _descriptionBox.Text = _wizard.Description;
                _content.Controls.Add(Wrap(
                    new Label { Text = "Название:", AutoSize = true },
                    _titleBox,
                    new Label { Text = "Описание:", AutoSize = true, Margin = new Padding(0, 12, 0, 0) },
                    _descriptionBox));
                break;

            case WizardStep.Preview:
                _stepTitle.Text = "Шаг 4 из 5 — Картинка (необязательно)";
                _hint.Text = "Можно выбрать превью для Workshop или нажать «Пропустить».";
                _previewLabel.Text = string.IsNullOrEmpty(_wizard.PreviewImagePath)
                    ? "Картинка не выбрана."
                    : $"Выбрано: {_wizard.PreviewImagePath}";
                _content.Controls.Add(Wrap(_btnPickPreview, _btnSkipPreview, _previewLabel));
                break;

            case WizardStep.Confirm:
                _stepTitle.Text = "Шаг 5 из 5 — Готово к загрузке";
                _hint.Text = "Проверьте данные и нажмите «Загрузить». Нужен запущенный Steam.";
                _summaryLabel.Text =
                    $"Папка: {_wizard.ModFolder}\n" +
                    $"AppID: {_wizard.AppId}\n" +
                    $"Название: {_wizard.Title}\n" +
                    $"Описание: {_wizard.Description}\n" +
                    $"Превью: {(_wizard.PreviewImagePath ?? "нет")}";
                _content.Controls.Add(_summaryLabel);
                break;
        }

        RenderNavOnly();
        _status.Text = _wizard.LastMessage ?? " ";
    }

    private void RenderNavOnly()
    {
        _btnBack.Enabled = _wizard.CanGoBack;
        _btnNext.Visible = _wizard.CurrentStep != WizardStep.Confirm;
        _btnNext.Enabled = _wizard.CanGoNext;
        _btnUpload.Visible = _wizard.CurrentStep == WizardStep.Confirm;
        _btnLaunchSteam.Visible = true;
    }

    private static Control Wrap(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        foreach (var c in controls)
            panel.Controls.Add(c);
        return panel;
    }

    private void ShowHumanError(string message)
    {
        MessageBox.Show(this, message, "Что не так", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _status.Text = message;
    }
}

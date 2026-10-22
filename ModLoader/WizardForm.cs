using ModLoader.Core;
using ModLoader.Core.Steam;
using ModLoader.Core.Wizard;

namespace ModLoader;

/// <summary>
/// Пошаговый мастер: короткие лейблы, детали в FAQ.
/// Выбор: Steam-клиент (Steamworks) или запасной steamcmd.
/// </summary>
public sealed class WizardForm : Form
{
    private static readonly Color Bg = Color.FromArgb(246, 247, 249);
    private static readonly Color Card = Color.White;
    private static readonly Color TextPrimary = Color.FromArgb(28, 35, 45);
    private static readonly Color TextMuted = Color.FromArgb(95, 105, 120);
    private static readonly Color Accent = Color.FromArgb(30, 120, 90);
    private static readonly Color AccentSoft = Color.FromArgb(220, 240, 232);
    private static readonly Color DangerSoft = Color.FromArgb(255, 236, 232);
    private static readonly Color Border = Color.FromArgb(220, 224, 230);
    private static readonly Color SteamOk = Color.FromArgb(20, 130, 80);

    private readonly WizardStateMachine _wizard = new();
    private readonly PublishService _publish;
    private readonly ISteamClient _steam;
    private readonly System.Windows.Forms.Timer _steamPollTimer;

    private readonly Label _stepTitle = new();
    private readonly Label _hint = new();
    private readonly Label _status = new();
    private readonly Label _steamBadge = new();
    private readonly Label _steamCmdBadge = new();
    private readonly Panel _progressPanel = new();
    private readonly Panel _contentCard = new();
    private readonly Panel _content = new();
    private readonly Button _btnBack = new();
    private readonly Button _btnNext = new();
    private readonly Button _btnUpload = new();
    private readonly Button _btnLaunchSteam = new();
    private readonly Button _btnFaq = new();
    private readonly Button _btnPickSteamCmd = new();
    private readonly Button _btnInstallSteamCmd = new();
    private readonly Button _btnPickSteamCmdLarge = new();
    private readonly Button _btnInstallSteamCmdLarge = new();
    private readonly SteamCmdInstaller _steamCmdInstaller;
    private bool _steamCmdBusy;

    private readonly Button _btnPickFolder = new();
    private readonly Label _folderLabel = new();
    private readonly FlowLayoutPanel _gamesPanel = new();
    private readonly NumericUpDown _appIdInput = new();
    private readonly Button _btnNewMod = new();
    private readonly Button _btnUpdateMod = new();
    private readonly Button _btnMethodSteamClient = new();
    private readonly Button _btnMethodSteamCmd = new();
    private readonly Panel _workshopIdPanel = new();
    private readonly TextBox _workshopIdBox = new();
    private readonly Button _btnPasteWorkshopId = new();
    private readonly TextBox _titleBox = new();
    private readonly TextBox _descriptionBox = new();
    private readonly Button _btnPickPreview = new();
    private readonly Button _btnSkipPreview = new();
    private readonly Label _previewLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly TextBox _steamLoginBox = new();
    private readonly Label _steamCmdPathLabel = new();
    private readonly Panel _steamCmdConfirmPanel = new();

    public WizardForm(
        ISteamClient steam,
        SteamCmdInstaller? steamCmdInstaller = null,
        IWorkshopPublisher? workshopPublisher = null)
    {
        _steam = steam;
        _publish = new PublishService(steam, workshopPublisher: workshopPublisher);
        _steamCmdInstaller = steamCmdInstaller ?? SteamCmdInstaller.CreateDefault();

        Text = "ModLoader — Steam Workshop";
        Width = 780;
        Height = 640;
        MinimumSize = new Size(700, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10.5F);
        BackColor = Bg;
        ForeColor = TextPrimary;

        BuildChrome();
        BuildStepControls();
        WireEvents();

        _steamPollTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _steamPollTimer.Tick += (_, _) => RefreshSteamUi();
        _steamPollTimer.Start();

        Render();
        RefreshSteamUi();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _steamPollTimer.Stop();
        _steamPollTimer.Dispose();
        base.OnFormClosed(e);
    }

    private void BuildChrome()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 118,
            BackColor = Card,
            Padding = new Padding(20, 14, 20, 10)
        };

        var brand = new Label
        {
            Text = "ModLoader",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11F),
            ForeColor = Accent,
            Location = new Point(20, 12)
        };

        StyleSecondaryButton(_btnFaq, "FAQ");
        _btnFaq.Size = new Size(72, 32);
        _btnFaq.Location = new Point(620, 8);
        _btnFaq.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _stepTitle.AutoSize = false;
        _stepTitle.Font = new Font("Segoe UI Semibold", 16F);
        _stepTitle.ForeColor = TextPrimary;
        _stepTitle.Location = new Point(20, 38);
        _stepTitle.Size = new Size(660, 32);

        _hint.AutoSize = false;
        _hint.Font = new Font("Segoe UI", 10F);
        _hint.ForeColor = TextMuted;
        _hint.Location = new Point(20, 72);
        _hint.Size = new Size(660, 36);

        _progressPanel.Dock = DockStyle.Bottom;
        _progressPanel.Height = 18;
        _progressPanel.BackColor = Card;
        _progressPanel.Padding = new Padding(20, 0, 20, 8);

        header.Controls.Add(brand);
        header.Controls.Add(_btnFaq);
        header.Controls.Add(_stepTitle);
        header.Controls.Add(_hint);
        header.Controls.Add(_progressPanel);

        var steamBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 96,
            BackColor = AccentSoft,
            Padding = new Padding(12, 6, 12, 6)
        };

        var steamRow = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = AccentSoft };
        var cmdRow = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AccentSoft };

        _steamBadge.AutoSize = true;
        _steamBadge.Font = new Font("Segoe UI Semibold", 10F);
        _steamBadge.ForeColor = SteamOk;
        _steamBadge.Dock = DockStyle.Left;
        _steamBadge.TextAlign = ContentAlignment.MiddleLeft;
        _steamBadge.Padding = new Padding(4, 8, 12, 0);

        _steamCmdBadge.AutoSize = true;
        _steamCmdBadge.Font = new Font("Segoe UI Semibold", 10F);
        _steamCmdBadge.ForeColor = TextMuted;
        _steamCmdBadge.Dock = DockStyle.Left;
        _steamCmdBadge.TextAlign = ContentAlignment.MiddleLeft;
        _steamCmdBadge.Padding = new Padding(4, 10, 8, 0);

        StyleSecondaryButton(_btnLaunchSteam, "Запустить Steam");
        _btnLaunchSteam.Dock = DockStyle.Right;
        _btnLaunchSteam.Width = 150;
        _btnLaunchSteam.Height = 34;

        StylePrimaryButton(_btnPickSteamCmd, "Указать папку steamcmd…");
        _btnPickSteamCmd.Dock = DockStyle.Right;
        _btnPickSteamCmd.Width = 230;
        _btnPickSteamCmd.Height = 36;
        _btnPickSteamCmd.Font = new Font("Segoe UI Semibold", 9.5F);

        StyleSecondaryButton(_btnInstallSteamCmd, "Скачать и установить steamcmd");
        _btnInstallSteamCmd.Dock = DockStyle.Right;
        _btnInstallSteamCmd.Width = 250;
        _btnInstallSteamCmd.Height = 36;
        _btnInstallSteamCmd.Font = new Font("Segoe UI Semibold", 9.5F);

        steamRow.Controls.Add(_btnLaunchSteam);
        steamRow.Controls.Add(_steamBadge);

        // Dock.Right: первый добавленный — у правого края (главное: указать папку)
        cmdRow.Controls.Add(_btnPickSteamCmd);
        cmdRow.Controls.Add(_btnInstallSteamCmd);
        cmdRow.Controls.Add(_steamCmdBadge);

        steamBar.Controls.Add(cmdRow);
        steamBar.Controls.Add(steamRow);

        _contentCard.Dock = DockStyle.Fill;
        _contentCard.Padding = new Padding(20, 16, 20, 8);
        _contentCard.BackColor = Bg;

        _content.Dock = DockStyle.Fill;
        _content.BackColor = Card;
        _content.Padding = new Padding(20);
        _contentCard.Controls.Add(_content);

        _status.AutoSize = false;
        _status.Dock = DockStyle.Bottom;
        _status.Height = 52;
        _status.Padding = new Padding(20, 6, 20, 6);
        _status.ForeColor = TextMuted;
        _status.BackColor = Bg;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 8, 12, 8),
            WrapContents = false,
            BackColor = Card
        };

        StylePrimaryButton(_btnUpload, "Загрузить");
        StylePrimaryButton(_btnNext, "Далее →");
        StyleSecondaryButton(_btnBack, "← Назад");

        buttons.Controls.Add(_btnUpload);
        buttons.Controls.Add(_btnNext);
        buttons.Controls.Add(_btnBack);

        Controls.Add(_contentCard);
        Controls.Add(buttons);
        Controls.Add(_status);
        Controls.Add(steamBar);
        Controls.Add(header);
    }

    private static void StylePrimaryButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(150, 40);
        button.Margin = new Padding(8, 4, 8, 4);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Accent;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 10.5F);
        button.Cursor = Cursors.Hand;
    }

    private static void StyleSecondaryButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(150, 40);
        button.Margin = new Padding(8, 4, 8, 4);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = Color.White;
        button.ForeColor = TextPrimary;
        button.Font = new Font("Segoe UI", 10F);
        button.Cursor = Cursors.Hand;
    }

    private static void StyleChoiceButton(Button button, string text, bool selected)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(280, 64);
        button.Margin = new Padding(0, 0, 12, 12);
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Segoe UI Semibold", 11F);
        button.Cursor = Cursors.Hand;
        button.TextAlign = ContentAlignment.MiddleCenter;
        if (selected)
        {
            button.BackColor = AccentSoft;
            button.ForeColor = Accent;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.BorderSize = 2;
        }
        else
        {
            button.BackColor = Color.White;
            button.ForeColor = TextPrimary;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
        }
    }

    private void BuildStepControls()
    {
        StylePrimaryButton(_btnPickFolder, "Выбрать папку мода…");
        _btnPickFolder.Width = 240;
        _folderLabel.AutoSize = true;
        _folderLabel.MaximumSize = new Size(620, 0);
        _folderLabel.ForeColor = TextMuted;
        _folderLabel.Padding = new Padding(0, 12, 0, 0);

        _gamesPanel.FlowDirection = FlowDirection.TopDown;
        _gamesPanel.AutoSize = true;
        _gamesPanel.WrapContents = false;
        _gamesPanel.Dock = DockStyle.Fill;

        foreach (var (name, appId) in KnownGames.All)
        {
            var id = appId;
            var btn = new Button();
            StyleChoiceButton(btn, $"{name}\nAppID {appId}", selected: false);
            btn.Width = 300;
            btn.Height = 56;
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
            Text = "Другой AppID:",
            AutoSize = true,
            ForeColor = TextMuted,
            Margin = new Padding(0, 16, 0, 6)
        };
        _appIdInput.Maximum = uint.MaxValue;
        _appIdInput.Minimum = 0;
        _appIdInput.Width = 160;
        _appIdInput.Font = new Font("Segoe UI", 12F);
        _appIdInput.ValueChanged += (_, _) =>
        {
            _wizard.SetAppId((uint)_appIdInput.Value);
            RenderNavOnly();
            RefreshProgress();
        };
        _gamesPanel.Controls.Add(otherLabel);
        _gamesPanel.Controls.Add(_appIdInput);

        StyleChoiceButton(_btnNewMod, "Новый мод", false);
        StyleChoiceButton(_btnUpdateMod, "Обновить существующий", false);

        StyleChoiceButton(_btnMethodSteamClient, "Через Steam-клиент\n(без пароля)", true);
        StyleChoiceButton(_btnMethodSteamCmd, "Через steamcmd\n(логин / пароль)", false);
        _btnMethodSteamClient.Width = 300;
        _btnMethodSteamClient.Height = 72;
        _btnMethodSteamCmd.Width = 300;
        _btnMethodSteamCmd.Height = 72;

        _workshopIdBox.PlaceholderText = "Workshop ID или ссылка";
        _workshopIdBox.Width = 420;
        _workshopIdBox.Font = new Font("Segoe UI", 11F);
        _workshopIdBox.TextChanged += (_, _) =>
        {
            _wizard.SetWorkshopIdInput(_workshopIdBox.Text);
            RenderNavOnly();
            _status.Text = _wizard.LastMessage ?? " ";
        };

        StyleSecondaryButton(_btnPasteWorkshopId, "Вставить");
        _btnPasteWorkshopId.Width = 120;
        _btnPasteWorkshopId.Height = 36;

        _workshopIdPanel.AutoSize = true;
        var idRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 10, 0, 0)
        };
        idRow.Controls.Add(_workshopIdBox);
        idRow.Controls.Add(_btnPasteWorkshopId);
        _workshopIdPanel.Controls.Add(idRow);

        _titleBox.PlaceholderText = "Название";
        _titleBox.Width = 520;
        _titleBox.Font = new Font("Segoe UI", 11F);
        _titleBox.TextChanged += (_, _) =>
        {
            _wizard.SetTitleAndDescription(_titleBox.Text, _descriptionBox.Text);
            RenderNavOnly();
        };

        _descriptionBox.PlaceholderText = "Описание";
        _descriptionBox.Multiline = true;
        _descriptionBox.Height = 110;
        _descriptionBox.Width = 520;
        _descriptionBox.Font = new Font("Segoe UI", 11F);
        _descriptionBox.TextChanged += (_, _) =>
        {
            _wizard.SetTitleAndDescription(_titleBox.Text, _descriptionBox.Text);
            RenderNavOnly();
        };

        StylePrimaryButton(_btnPickPreview, "Выбрать картинку…");
        _btnPickPreview.Width = 200;
        StyleSecondaryButton(_btnSkipPreview, "Пропустить");
        _btnSkipPreview.Width = 140;
        _previewLabel.AutoSize = true;
        _previewLabel.ForeColor = TextMuted;
        _previewLabel.Padding = new Padding(0, 12, 0, 0);

        _summaryLabel.AutoSize = true;
        _summaryLabel.MaximumSize = new Size(600, 0);
        _summaryLabel.Font = new Font("Segoe UI", 10.5F);

        _steamLoginBox.PlaceholderText = "Логин Steam";
        _steamLoginBox.Width = 360;
        _steamLoginBox.Font = new Font("Segoe UI", 11F);

        _steamCmdPathLabel.AutoSize = true;
        _steamCmdPathLabel.ForeColor = TextMuted;
        _steamCmdPathLabel.MaximumSize = new Size(620, 0);

        StylePrimaryButton(_btnPickSteamCmdLarge, "Указать папку steamcmd…");
        _btnPickSteamCmdLarge.Width = 320;
        _btnPickSteamCmdLarge.Height = 48;
        _btnPickSteamCmdLarge.Font = new Font("Segoe UI Semibold", 11F);

        StyleSecondaryButton(_btnInstallSteamCmdLarge, "Скачать и установить steamcmd");
        _btnInstallSteamCmdLarge.Width = 320;
        _btnInstallSteamCmdLarge.Height = 48;
        _btnInstallSteamCmdLarge.Font = new Font("Segoe UI Semibold", 11F);

        _steamCmdConfirmPanel.AutoSize = true;
        _steamCmdConfirmPanel.Controls.Add(Wrap(
            FieldLabel("Логин Steam", top: 8),
            _steamLoginBox,
            FieldLabel("steamcmd", top: 12),
            _steamCmdPathLabel,
            _btnPickSteamCmdLarge,
            _btnInstallSteamCmdLarge));
    }

    private void WireEvents()
    {
        _btnFaq.Click += (_, _) => FaqHelper.Open(this);

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

        _btnPickFolder.Click += (_, _) => PickModFolder();

        _btnNewMod.Click += (_, _) =>
        {
            _wizard.SetPublishKindCreateNew();
            Render();
        };

        _btnUpdateMod.Click += (_, _) =>
        {
            _wizard.SetPublishKindUpdate();
            Render();
        };

        _btnMethodSteamClient.Click += (_, _) =>
        {
            _wizard.SetUploadMethodSteamClient();
            Render();
        };

        _btnMethodSteamCmd.Click += (_, _) =>
        {
            _wizard.SetUploadMethodSteamCmd();
            Render();
        };

        _btnPasteWorkshopId.Click += (_, _) =>
        {
            try
            {
                if (Clipboard.ContainsText())
                    _workshopIdBox.Text = Clipboard.GetText().Trim();
            }
            catch
            {
                ShowHumanError("Не удалось прочитать буфер. Вставьте вручную (Ctrl+V).");
            }
        };

        _btnPickPreview.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Превью Workshop",
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
        _btnPickSteamCmd.Click += (_, _) => PickSteamCmd();
        _btnPickSteamCmdLarge.Click += (_, _) => PickSteamCmd();
        _btnInstallSteamCmd.Click += async (_, _) => await InstallSteamCmdAsync();
        _btnInstallSteamCmdLarge.Click += async (_, _) => await InstallSteamCmdAsync();
        _btnUpload.Click += async (_, _) => await DoUploadAsync();
    }

    private void PickModFolder()
    {
        var initial = GameModsPathFinder.SuggestInitialDirectory(
            _steam.FindSteamPath(),
            _wizard.AppId > 0 ? _wizard.AppId : GameModsPathFinder.DoorKickers2AppId);

        using var dialog = new FolderBrowserDialog
        {
            Description = "Папка одного мода (с mod.xml)",
            UseDescriptionForTitle = true
        };
        if (!string.IsNullOrWhiteSpace(initial))
            dialog.InitialDirectory = initial;

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (!_wizard.SetModFolder(dialog.SelectedPath))
        {
            ShowHumanError(_wizard.LastMessage ?? "Не удалось выбрать папку.");
            Render();
            return;
        }

        if (_wizard.LastMessage is not null &&
            _wizard.LastMessage.Contains("mod.xml", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, _wizard.LastMessage + "\n\nПодробности — FAQ.", "Подсказка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        Render();
    }

    private void PickSteamCmd()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Выберите папку, где лежит steamcmd.exe (или папку уровнем выше)",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _publish.SetSteamCmdPath(dialog.SelectedPath);
        RefreshSteamUi();
        var ui = _publish.GetSteamCmdUiStatus();
        _status.Text = ui.IsFound
            ? $"steamcmd: {ui.Path}"
            : "В папке (и на 1 уровень вглубь) нет steamcmd.exe. См. FAQ.";
        if (!ui.IsFound)
            ShowHumanError(_status.Text);
    }

    private async Task InstallSteamCmdAsync()
    {
        if (_steamCmdBusy)
            return;

        var startPath = SteamCmdInstaller.EnsureDefaultInstallDirectoryExists();
        using (var dialog = new FolderBrowserDialog
        {
            Description = "Куда установить steamcmd?",
            UseDescriptionForTitle = true,
            SelectedPath = startPath
        })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var installDir = dialog.SelectedPath;
            _steamCmdBusy = true;
            SetSteamCmdButtonsEnabled(false);
            _status.BackColor = Bg;
            _status.Text = "Идёт установка…";

            try
            {
                var progress = new Progress<string>(s =>
                {
                    if (IsHandleCreated && !IsDisposed)
                        BeginInvoke(() => _status.Text = s);
                });

                var result = await _steamCmdInstaller
                    .InstallAsync(installDirectory: installDir, progress: progress)
                    .ConfigureAwait(true);
                if (result.Success && result.SteamCmdExePath is not null)
                {
                    _publish.SetSteamCmdPath(result.SteamCmdExePath);
                    RefreshSteamUi();
                    _status.Text = result.Message;
                    _status.BackColor = AccentSoft;
                }
                else
                {
                    ShowSteamCmdInstallError(result);
                }
            }
            finally
            {
                _steamCmdBusy = false;
                SetSteamCmdButtonsEnabled(true);
                RefreshSteamUi();
            }
        }
    }

    private void SetSteamCmdButtonsEnabled(bool enabled)
    {
        _btnPickSteamCmd.Enabled = enabled;
        _btnInstallSteamCmd.Enabled = enabled;
        _btnPickSteamCmdLarge.Enabled = enabled;
        _btnInstallSteamCmdLarge.Enabled = enabled;
    }

    private void RefreshSteamUi()
    {
        var steam = _publish.GetSteamUiStatus();
        _steamBadge.Text = steam.StatusText;
        _steamBadge.ForeColor = steam.IsRunning ? SteamOk : Color.FromArgb(160, 90, 40);
        _btnLaunchSteam.Visible = steam.ShowLaunchButton;
        _btnLaunchSteam.Enabled = steam.ShowLaunchButton && !_steamCmdBusy;

        var cmd = _publish.GetSteamCmdUiStatus();
        _steamCmdBadge.Text = cmd.StatusText;
        _steamCmdBadge.ForeColor = cmd.IsFound ? SteamOk : Color.FromArgb(160, 90, 40);

        // «Указать папку» всегда; «Скачать» — если ещё не найден (или можно переустановить — скрываем при успехе)
        _btnPickSteamCmd.Visible = true;
        _btnInstallSteamCmd.Visible = !cmd.IsFound;
        _btnPickSteamCmdLarge.Visible = true;
        _btnInstallSteamCmdLarge.Visible = !cmd.IsFound;

        _steamCmdPathLabel.Text = cmd.Path is null
            ? "steamcmd не найден — укажите папку или скачайте."
            : cmd.Path;
    }

    private void TryLaunchSteam()
    {
        var confirm = MessageBox.Show(
            this,
            "Открыть Steam?",
            "Steam",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        var result = _publish.EnsureSteamReady(allowLaunch: true);
        _status.Text = result.Message;
        RefreshSteamUi();
        if (!result.IsReady && !result.Message.Contains("запускается", StringComparison.OrdinalIgnoreCase))
            ShowHumanError(result.Message);
    }

    private async Task DoUploadAsync()
    {
        var parameters = _wizard.BuildUploadParameters();
        if (!parameters.IsSuccess)
        {
            ShowHumanError(parameters.Error ?? "Проверьте поля. См. FAQ.");
            return;
        }

        RefreshSteamUi();
        if (!_steam.IsRunning())
        {
            var ask = MessageBox.Show(
                this,
                "Steam не запущен. Открыть Steam?",
                "Steam",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ask != DialogResult.Yes)
            {
                _status.Text = "Нужен запущенный Steam. См. FAQ.";
                return;
            }

            var ready = _publish.EnsureSteamReady(allowLaunch: true);
            _status.Text = ready.Message;
            RefreshSteamUi();
            if (!_steam.IsRunning())
            {
                MessageBox.Show(this, "Дождитесь входа в Steam и нажмите «Загрузить» снова.",
                    "Steam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        if (_wizard.UploadMethod == UploadMethod.SteamCmd)
        {
            var steamUser = _steamLoginBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(steamUser))
            {
                ShowHumanError("Введите логин Steam. Пароль и Guard — в окне steamcmd. См. FAQ.");
                return;
            }

            if (!_publish.GetSteamCmdUiStatus().IsFound)
            {
                ShowHumanError(
                    "Не найден steamcmd.exe. Нажмите «Указать папку steamcmd…» или «Скачать и установить steamcmd». См. FAQ.");
                return;
            }

            var goCmd = MessageBox.Show(
                this,
                "Запустить steamcmd для загрузки в Workshop?\n(Игра не откроется.)",
                "Загрузка",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (goCmd != DialogResult.Yes)
                return;

            _status.Text = "Запуск steamcmd…";
            var cmdResult = await _publish
                .PublishAsync(parameters.Value!, UploadMethod.SteamCmd, steamUser)
                .ConfigureAwait(true);
            ShowPublishOutcome(cmdResult);
            return;
        }

        var goClient = MessageBox.Show(
            this,
            "Загрузить через Steam-клиент?\n(Без пароля, аккаунт из запущенного Steam.)",
            "Загрузка",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (goClient != DialogResult.Yes)
            return;

        _btnUpload.Enabled = false;
        _status.Text = "Загрузка через Steam-клиент…";
        try
        {
            var progress = new Progress<float>(p =>
            {
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(() => _status.Text = $"Загрузка… {(int)(p * 100)}%");
            });
            var result = await _publish
                .PublishAsync(parameters.Value!, UploadMethod.SteamClient, progress: progress)
                .ConfigureAwait(true);
            ShowPublishOutcome(result);
        }
        finally
        {
            _btnUpload.Enabled = true;
        }
    }

    private void ShowPublishOutcome(PublishResult result)
    {
        _status.Text = result.Message.Replace("\n", " ");
        if (result.Success)
            _status.BackColor = AccentSoft;
        else if (result.OfferFaq)
            _status.BackColor = DangerSoft;

        if (!result.Success && result.OfferFaq)
        {
            var ask = MessageBox.Show(
                this,
                result.Message + "\n\nОткрыть FAQ?",
                "Не получилось",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (ask == DialogResult.Yes)
                FaqHelper.Open(this);
            return;
        }

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
        RefreshProgress();

        switch (_wizard.CurrentStep)
        {
            case WizardStep.ModFolder:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Папка мода";
                _hint.Text = "Подпапка одного мода с mod.xml (не корень mods). FAQ — детали.";
                _content.Controls.Add(Wrap(_btnPickFolder, _folderLabel));
                _folderLabel.Text = string.IsNullOrEmpty(_wizard.ModFolder)
                    ? "Папка не выбрана."
                    : _wizard.ModFolder;
                break;

            case WizardStep.GameAppId:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Игра";
                _hint.Text = "Door Kickers 2 = 1239080.";
                HighlightGameButtons();
                _content.Controls.Add(_gamesPanel);
                if (_wizard.AppId > 0)
                    _appIdInput.Value = _wizard.AppId;
                break;

            case WizardStep.PublishKind:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Новый или обновить";
                _hint.Text = "Новый item или обновление по Workshop ID.";
                StyleChoiceButton(_btnNewMod, "Новый мод", _wizard.PublishKind == PublishKind.CreateNew);
                StyleChoiceButton(_btnUpdateMod, "Обновить существующий",
                    _wizard.PublishKind == PublishKind.UpdateExisting);
                _workshopIdPanel.Visible = _wizard.PublishKind == PublishKind.UpdateExisting;
                _workshopIdBox.Text = _wizard.WorkshopIdInput;
                _content.Controls.Add(Wrap(_btnNewMod, _btnUpdateMod, _workshopIdPanel));
                break;

            case WizardStep.TitleDescription:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Название";
                _hint.Text = "Название и описание в Workshop.";
                _titleBox.Text = _wizard.Title;
                _descriptionBox.Text = _wizard.Description;
                _content.Controls.Add(Wrap(
                    FieldLabel("Название"),
                    _titleBox,
                    FieldLabel("Описание", top: 14),
                    _descriptionBox));
                break;

            case WizardStep.Preview:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Превью";
                _hint.Text = "Часто mod_image.jpg в папке мода.";
                _previewLabel.Text = string.IsNullOrEmpty(_wizard.PreviewImagePath)
                    ? "Не выбрано."
                    : _wizard.PreviewImagePath;
                _content.Controls.Add(Wrap(_btnPickPreview, _btnSkipPreview, _previewLabel));
                break;

            case WizardStep.UploadMethod:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Как загрузить";
                _hint.Text = "Две кнопки. Детали — FAQ.";
                StyleChoiceButton(
                    _btnMethodSteamClient,
                    "Через Steam-клиент\n(без пароля)",
                    _wizard.UploadMethod == UploadMethod.SteamClient);
                StyleChoiceButton(
                    _btnMethodSteamCmd,
                    "Через steamcmd\n(логин / пароль)",
                    _wizard.UploadMethod == UploadMethod.SteamCmd);
                _content.Controls.Add(Wrap(_btnMethodSteamClient, _btnMethodSteamCmd));
                break;

            case WizardStep.Confirm:
                _stepTitle.Text = $"Шаг {_wizard.StepNumber} из {_wizard.TotalSteps} — Загрузка";
                var viaClient = _wizard.UploadMethod == UploadMethod.SteamClient;
                _hint.Text = viaClient
                    ? "Steam-клиент уже вошёл — пароль не нужен."
                    : "steamcmd: логин здесь, пароль в его окне.";
                _summaryLabel.Text =
                    $"Папка: {_wizard.ModFolder}\n" +
                    $"AppID: {_wizard.AppId}\n" +
                    $"Режим: {(_wizard.PublishKind == PublishKind.UpdateExisting ? $"обновление ({_wizard.PublishedFileId})" : "новый")}\n" +
                    $"Способ: {(viaClient ? "Steam-клиент" : "steamcmd")}\n" +
                    $"Название: {_wizard.Title}\n" +
                    $"Превью: {(_wizard.PreviewImagePath ?? "нет")}";
                if (viaClient)
                {
                    _content.Controls.Add(Wrap(_summaryLabel));
                }
                else
                {
                    _content.Controls.Add(Wrap(_summaryLabel, _steamCmdConfirmPanel));
                }

                break;
        }

        RenderNavOnly();
        RefreshSteamUi();
        _status.BackColor = Bg;
        _status.Text = _wizard.LastMessage ?? " ";
    }

    private void HighlightGameButtons()
    {
        var index = 0;
        foreach (var (name, appId) in KnownGames.All)
        {
            if (index < _gamesPanel.Controls.Count && _gamesPanel.Controls[index] is Button btn)
                StyleChoiceButton(btn, $"{name}\nAppID {appId}", _wizard.AppId == appId);
            index++;
        }
    }

    private void RefreshProgress()
    {
        _progressPanel.Controls.Clear();
        var total = _wizard.TotalSteps;
        var current = _wizard.StepNumber;
        var x = 20;
        for (var i = 1; i <= total; i++)
        {
            var done = i < current;
            var active = i == current;
            var dot = new Panel
            {
                Width = done || active ? 28 : 10,
                Height = 10,
                Left = x,
                Top = 2,
                BackColor = active || done ? Accent : Border
            };
            _progressPanel.Controls.Add(dot);
            x += dot.Width + 6;
        }
    }

    private void RenderNavOnly()
    {
        _btnBack.Enabled = _wizard.CanGoBack;
        _btnNext.Visible = _wizard.CurrentStep != WizardStep.Confirm;
        _btnNext.Enabled = _wizard.CanGoNext;
        _btnUpload.Visible = _wizard.CurrentStep == WizardStep.Confirm;
    }

    private static Label FieldLabel(string text, int top = 0) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 10F),
        ForeColor = TextPrimary,
        Margin = new Padding(0, top, 0, 4)
    };

    private static Control Wrap(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Card
        };
        foreach (var c in controls)
            panel.Controls.Add(c);
        return panel;
    }

    private void ShowHumanError(string message)
    {
        MessageBox.Show(this, message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _status.Text = message.Replace("\n", " ");
        _status.BackColor = DangerSoft;
    }

    /// <summary>
    /// Короткая ошибка установки steamcmd; при сетевом/bootstrap сбое — предложить FAQ.
    /// </summary>
    private void ShowSteamCmdInstallError(SteamCmdInstallResult result)
    {
        _status.Text = result.Message.Replace("\n", " ");
        _status.BackColor = DangerSoft;

        if (!result.OfferFaq)
        {
            MessageBox.Show(this, result.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var ask = MessageBox.Show(
            this,
            result.Message + "\n\nОткрыть FAQ?",
            "Ошибка",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (ask == DialogResult.Yes)
            FaqHelper.Open(this);
    }
}

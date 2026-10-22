using System.Diagnostics;

namespace ModLoader;

/// <summary>
/// Открывает FAQ.md рядом с exe (или показывает встроенный краткий текст).
/// </summary>
internal static class FaqHelper
{
    private const string FileName = "FAQ.md";

    public static string? FindFaqPath()
    {
        var nextToExe = Path.Combine(AppContext.BaseDirectory, FileName);
        if (File.Exists(nextToExe))
            return nextToExe;

        // Dev: рядом с репозиторием / проектной папкой
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, FileName);
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return null;
    }

    public static void Open(IWin32Window owner)
    {
        var path = FindFaqPath();
        if (path is not null)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                return;
            }
            catch
            {
                // fall through to MessageBox
            }

            try
            {
                using var form = new Form
                {
                    Text = "FAQ / Справка",
                    Width = 720,
                    Height = 560,
                    StartPosition = FormStartPosition.CenterParent,
                    Font = new Font("Segoe UI", 10F)
                };
                var box = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 10F),
                    Text = File.ReadAllText(path)
                };
                form.Controls.Add(box);
                form.ShowDialog(owner);
                return;
            }
            catch
            {
                // fall through
            }
        }

        MessageBox.Show(
            owner,
            BuiltInSummary,
            "FAQ / Справка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private const string BuiltInSummary =
        "ModLoader публикует мод через steamcmd (workshop_build_item), без запуска игры.\n\n" +
        "• Нужны Steam и steamcmd.exe («Указать папку steamcmd…» или «Скачать и установить»).\n" +
        "• Автоустановка: %LOCALAPPDATA%\\ModLoader\\steamcmd\\\n" +
        "• Уже ставили — найдите steamcmd.exe в Проводнике и укажите папку.\n" +
        "• В приложении — только логин; пароль и Steam Guard — в окне steamcmd.\n" +
        "• Новый мод: publishedfileid = 0. Обновление: Workshop ID из ссылки.\n" +
        "• Door Kickers = 248610, Door Kickers 2 = 1239080.\n" +
        "• http error 0 при установке steamcmd — сеть/CDN/firewall; см. FAQ, не пути ModLoader.\n\n" +
        "Полный FAQ.md рядом с ModLoader.exe.";
}

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
        "ModLoader: Steam-клиент (Steamworks) или steamcmd; игра не запускается.\n\n" +
        "• Официальный KillHouse/DK2: мод в mods или mods_upload (лучше mods_upload); в игре — синяя стрелка Upload.\n" +
        "• В мастере — подпапка мода с mod.xml; contentfolder = эта папка.\n" +
        "• steamcmd: «Указать папку…» / «Скачать и установить»; логин в UI, пароль/Guard в окне steamcmd.\n" +
        "• Door Kickers 2 = 1239080. http error 0 — сеть/CDN; детали в FAQ.md рядом с exe.";
}

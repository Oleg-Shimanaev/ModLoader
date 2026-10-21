using ModLoader.Core.Steam;

namespace ModLoader;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new WizardForm(new SteamClient()));
    }
}

namespace ModLoader.Core.Steam;

public static class PublishMessages
{
    public const string NoLicenseHint =
        "Steam пишет «нет лицензии» — обычно игра не куплена на этом аккаунте или выбран неверный AppID. См. FAQ.";

    public const string GameNotInstalled =
        NoLicenseHint +
        "\n\nDoor Kickers = 248610, Door Kickers 2 = 1239080.";

    public const string LaunchFailed =
        "Не удалось запустить игру через Steam (legacy).\n\n" + NoLicenseHint;
}

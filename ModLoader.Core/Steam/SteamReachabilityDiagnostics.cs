namespace ModLoader.Core.Steam;

public enum SteamReachabilityFailureKind
{
    None = 0,
    NotRunning,
    ApiUnavailable,
    InitFailed,
    NotLoggedOn,
    Timeout,
    Unknown
}

/// <summary>
/// Понятные RU-сообщения, когда до Steam «не достучались».
/// </summary>
public static class SteamReachabilityDiagnostics
{
    public const string NotRunningUserMessage =
        "Не достучались до Steam: он не запущен.\n" +
        "Запустите Steam, войдите в аккаунт и повторите.\n" +
        "Проверьте интернет, VPN и firewall. См. FAQ.";

    public const string ApiUnavailableUserMessage =
        "Steamworks недоступен (нет steam_api64.dll).\n" +
        "Выберите «Через steamcmd» или переустановите ModLoader. См. FAQ.";

    public const string InitFailedUserMessage =
        "Не достучались до Steam: клиент не отвечает (Init failed).\n" +
        "Запустите Steam, проверьте интернет/VPN/firewall, перезапустите Steam. См. FAQ.";

    public const string NotLoggedOnUserMessage =
        "Steam запущен, но вход не выполнен.\n" +
        "Откройте Steam, войдите в аккаунт и повторите. См. FAQ.";

    public const string TimeoutUserMessage =
        "Нет связи со Steam (таймаут).\n" +
        "Проверьте интернет, VPN и firewall; перезапустите Steam. См. FAQ.";

    public const string UnknownUserMessage =
        "Ошибка связи со Steam.\n" +
        "Нужен запущенный Steam и право публиковать в Workshop. См. FAQ.";

    public static string UserMessage(SteamReachabilityFailureKind kind) =>
        kind switch
        {
            SteamReachabilityFailureKind.NotRunning => NotRunningUserMessage,
            SteamReachabilityFailureKind.ApiUnavailable => ApiUnavailableUserMessage,
            SteamReachabilityFailureKind.InitFailed => InitFailedUserMessage,
            SteamReachabilityFailureKind.NotLoggedOn => NotLoggedOnUserMessage,
            SteamReachabilityFailureKind.Timeout => TimeoutUserMessage,
            SteamReachabilityFailureKind.Unknown => UnknownUserMessage,
            _ => string.Empty
        };

    public static bool OfferFaq(SteamReachabilityFailureKind kind) =>
        kind is not SteamReachabilityFailureKind.None;
}

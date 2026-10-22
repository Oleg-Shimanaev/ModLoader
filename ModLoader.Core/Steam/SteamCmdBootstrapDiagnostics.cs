namespace ModLoader.Core.Steam;

public enum SteamCmdBootstrapFailureKind
{
    None = 0,
    HttpErrorZero,
    BootstrapFailed
}

/// <summary>
/// Распознаёт типичные сбои bootstrap steamcmd (+quit) по выводу / коду выхода.
/// </summary>
public static class SteamCmdBootstrapDiagnostics
{
    public const string HttpErrorZeroUserMessage =
        "Не удалось скачать пакет steamcmd (сеть/CDN/firewall). " +
        "Проверьте интернет, VPN, антивирус; повторите позже или запустите steamcmd.exe вручную. См. FAQ.";

    public const string BootstrapFailedUserMessage =
        "Установка steamcmd не завершилась (bootstrap). См. FAQ.";

    public static bool IsHttpErrorZero(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return false;

        return output.Contains("http error 0", StringComparison.OrdinalIgnoreCase);
    }

    public static SteamCmdBootstrapFailureKind Classify(string? output, int exitCode)
    {
        if (IsHttpErrorZero(output))
            return SteamCmdBootstrapFailureKind.HttpErrorZero;

        if (exitCode != 0)
            return SteamCmdBootstrapFailureKind.BootstrapFailed;

        // Частый спутник http error 0 без явной строки (битый package)
        if (!string.IsNullOrWhiteSpace(output)
            && output.Contains("missing or incorrect size", StringComparison.OrdinalIgnoreCase)
            && output.Contains("Download of package", StringComparison.OrdinalIgnoreCase))
        {
            return SteamCmdBootstrapFailureKind.HttpErrorZero;
        }

        return SteamCmdBootstrapFailureKind.None;
    }

    public static string UserMessage(SteamCmdBootstrapFailureKind kind) =>
        kind switch
        {
            SteamCmdBootstrapFailureKind.HttpErrorZero => HttpErrorZeroUserMessage,
            SteamCmdBootstrapFailureKind.BootstrapFailed => BootstrapFailedUserMessage,
            _ => string.Empty
        };
}

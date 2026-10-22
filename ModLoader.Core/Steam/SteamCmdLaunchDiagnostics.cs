namespace ModLoader.Core.Steam;

public enum SteamCmdLaunchFailureKind
{
    None = 0,
    NotFound,
    StartFailed,
    ExitedImmediately,
    TimeoutNoOutput
}

public sealed class SteamCmdLaunchAttempt
{
    public bool Success { get; init; }
    public SteamCmdLaunchFailureKind FailureKind { get; init; }

    public static SteamCmdLaunchAttempt Ok() =>
        new() { Success = true, FailureKind = SteamCmdLaunchFailureKind.None };

    public static SteamCmdLaunchAttempt Fail(SteamCmdLaunchFailureKind kind) =>
        new() { Success = false, FailureKind = kind };
}

/// <summary>
/// Классификация и RU-сообщения, когда консоль steamcmd не открывается.
/// </summary>
public static class SteamCmdLaunchDiagnostics
{
    public const int DefaultGraceMs = 2000;

    public const string NotFoundUserMessage =
        "Не найден steamcmd.exe.\n" +
        "Нажмите «Указать папку steamcmd…» или «Скачать и установить steamcmd». См. FAQ.";

    public const string StartFailedUserMessage =
        "Не удалось запустить steamcmd (консоль не открылась).\n" +
        "Проверьте антивирус, путь к steamcmd.exe и права. См. FAQ.";

    public const string ExitedImmediatelyUserMessage =
        "steamcmd сразу закрылся — консоль не открылась.\n" +
        "Антивирус мог заблокировать; запустите steamcmd.exe вручную. См. FAQ (и http error 0).";

    public const string TimeoutNoOutputUserMessage =
        "steamcmd завис без ответа (таймаут).\n" +
        "Проверьте сеть/антивирус или запустите steamcmd вручную. См. FAQ.";

    /// <summary>
    /// Чистая классификация по фактам запуска (удобно для тестов).
    /// </summary>
    public static SteamCmdLaunchFailureKind Classify(
        bool exeExists,
        bool startSucceeded,
        bool? processExitedWithinGrace = null,
        bool timedOutWaitingForOutput = false,
        string? output = null)
    {
        if (!exeExists)
            return SteamCmdLaunchFailureKind.NotFound;

        if (!startSucceeded)
            return SteamCmdLaunchFailureKind.StartFailed;

        if (timedOutWaitingForOutput && string.IsNullOrWhiteSpace(output))
            return SteamCmdLaunchFailureKind.TimeoutNoOutput;

        if (processExitedWithinGrace == true)
            return SteamCmdLaunchFailureKind.ExitedImmediately;

        return SteamCmdLaunchFailureKind.None;
    }

    public static string UserMessage(SteamCmdLaunchFailureKind kind) =>
        kind switch
        {
            SteamCmdLaunchFailureKind.NotFound => NotFoundUserMessage,
            SteamCmdLaunchFailureKind.StartFailed => StartFailedUserMessage,
            SteamCmdLaunchFailureKind.ExitedImmediately => ExitedImmediatelyUserMessage,
            SteamCmdLaunchFailureKind.TimeoutNoOutput => TimeoutNoOutputUserMessage,
            _ => string.Empty
        };

    public static bool OfferFaq(SteamCmdLaunchFailureKind kind) =>
        kind is not SteamCmdLaunchFailureKind.None;
}

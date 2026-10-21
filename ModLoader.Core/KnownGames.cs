namespace ModLoader.Core;

/// <summary>
/// Известные игры с кнопками в UI (без ручного ввода AppID, если не нужно).
/// </summary>
public static class KnownGames
{
    public static IReadOnlyList<(string Name, uint AppId)> All { get; } =
    [
        ("Door Kickers", 248610),
        ("Door Kickers 2", 1231430)
    ];
}

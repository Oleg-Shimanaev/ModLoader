namespace ModLoader.Core.Steam;

public sealed class ProcessRunResult
{
    public int ExitCode { get; init; }
    public string CombinedOutput { get; init; } = string.Empty;
}

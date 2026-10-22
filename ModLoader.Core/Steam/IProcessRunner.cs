namespace ModLoader.Core.Steam;

/// <summary>
/// Запуск внешнего процесса с ожиданием завершения (для тестов мокается).
/// </summary>
public interface IProcessRunner
{
    /// <returns>Код выхода и объединённый stdout/stderr (если захват доступен).</returns>
    Task<ProcessRunResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken = default);
}

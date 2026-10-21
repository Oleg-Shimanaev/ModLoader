namespace ModLoader.Core.Validation;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public bool HasWarning { get; }
    public string Message { get; }

    private ValidationResult(bool isValid, bool hasWarning, string message)
    {
        IsValid = isValid;
        HasWarning = hasWarning;
        Message = message;
    }

    public static ValidationResult Ok(string message = "Папка подходит.") =>
        new(true, false, message);

    public static ValidationResult Warning(string message) =>
        new(true, true, message);

    public static ValidationResult Fail(string message) =>
        new(false, false, message);
}

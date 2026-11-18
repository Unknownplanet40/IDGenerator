namespace IdGen.Core.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, string> Errors { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static ValidationResult Success() => new();

    public static ValidationResult Failed(string field, string message)
    {
        var result = new ValidationResult();
        result.AddError(field, message);
        return result;
    }

    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = message;
        }
    }

    public string? GetError(string field) => Errors.GetValueOrDefault(field);
    public bool HasError(string field) => Errors.ContainsKey(field);
}

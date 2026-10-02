namespace AnonymizeDicom.Validation;

public sealed class ValidationReport
{
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public List<ValidationResult> Results { get; } = new();
}

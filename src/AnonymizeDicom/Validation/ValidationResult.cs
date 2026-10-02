namespace AnonymizeDicom.Validation;

public sealed record ValidationResult(string RuleName, bool Passed, string? Message = null);

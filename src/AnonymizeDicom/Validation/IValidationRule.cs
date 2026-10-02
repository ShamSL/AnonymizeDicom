using FellowOakDicom;

namespace AnonymizeDicom.Validation;

public interface IValidationRule
{
    string Name { get; }
    ValidationResult Validate(DicomFile source, DicomFile output);
}

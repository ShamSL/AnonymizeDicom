using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Validation.Rules;

public sealed class DicomIntegrityRule : IValidationRule
{
    public string Name => "DicomIntegrity";

    public ValidationResult Validate(DicomFile source, DicomFile output)
    {
        var missing = new List<string>();

        if (!output.Dataset.Contains(DicomTags.SOPClassUID.ToDicomTag()))
        {
            missing.Add("SOPClassUID");
        }

        if (!output.Dataset.Contains(DicomTags.SOPInstanceUID.ToDicomTag()))
        {
            missing.Add("SOPInstanceUID");
        }

        return missing.Count == 0
            ? new ValidationResult(Name, true)
            : new ValidationResult(Name, false, "Missing required tags: " + string.Join(", ", missing));
    }
}

using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Validation.Rules;

public sealed class WhitelistComplianceRule : IValidationRule
{
    private readonly AnonymizationProfile _profile;

    public WhitelistComplianceRule(AnonymizationProfile profile)
    {
        _profile = profile;
    }

    public string Name => "WhitelistCompliance";

    public ValidationResult Validate(DicomFile source, DicomFile output)
    {
        var offending = new List<string>();

        foreach (var item in output.Dataset)
        {
            var tag = DicomTagId.From(item.Tag);

            // group 0002 is the file meta info and is fine to keep
            if (tag.Group != 0x0002 && !_profile.IsWhitelisted(tag))
            {
                offending.Add(tag.ToString());
            }
        }

        return offending.Count == 0
            ? new ValidationResult(Name, true)
            : new ValidationResult(Name, false, "Non-whitelisted tags present: " + string.Join(", ", offending));
    }
}

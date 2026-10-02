using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Validation.Rules;

public sealed class PreservedTagsRule : IValidationRule
{
    private readonly AnonymizationProfile _profile;

    public PreservedTagsRule(AnonymizationProfile profile)
    {
        _profile = profile;
    }

    public string Name => "PreservedTags";

    public ValidationResult Validate(DicomFile source, DicomFile output)
    {
        var mismatches = new List<string>();

        foreach (var tag in _profile.WhitelistTags)
        {
            // These have their own dedicated rules.
            if (tag == DicomTags.PatientBirthDate || tag == DicomTags.StudyDate || tag == DicomTags.PixelData)
            {
                continue;
            }

            var dicomTag = tag.ToDicomTag();
            var sourceHas = source.Dataset.Contains(dicomTag);
            var outputHas = output.Dataset.Contains(dicomTag);

            if (sourceHas != outputHas)
            {
                mismatches.Add($"{tag} presence differs");
                continue;
            }

            if (sourceHas && !string.Equals(
                    GetString(source.Dataset, tag),
                    GetString(output.Dataset, tag),
                    StringComparison.Ordinal))
            {
                mismatches.Add($"{tag} value differs");
            }
        }

        return mismatches.Count == 0
            ? new ValidationResult(Name, true)
            : new ValidationResult(Name, false, string.Join("; ", mismatches));
    }

    private static string? GetString(DicomDataset dataset, DicomTagId tag)
        => dataset.TryGetString(tag.ToDicomTag(), out var value) ? value : null;
}

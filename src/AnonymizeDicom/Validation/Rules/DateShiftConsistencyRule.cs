using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Validation.Rules;

public sealed class DateShiftConsistencyRule : IValidationRule
{
    public string Name => "DateShiftConsistency";

    public ValidationResult Validate(DicomFile source, DicomFile output)
    {
        var errors = new List<string>();

        var sourceBirth = GetDate(source.Dataset, DicomTags.PatientBirthDate);
        var sourceStudy = GetDate(source.Dataset, DicomTags.StudyDate);

        if (sourceBirth is null && sourceStudy is null)
        {
            return new ValidationResult(Name, true, "No dates to verify");
        }

        if (sourceBirth is not null)
        {
            var outputBirth = GetDate(output.Dataset, DicomTags.PatientBirthDate);
            if (outputBirth is null || (sourceBirth.Value.Date - outputBirth.Value.Date).Days <= 0)
            {
                errors.Add("PatientBirthDate missing or not shifted");
            }
        }

        if (sourceStudy is not null)
        {
            var outputStudy = GetDate(output.Dataset, DicomTags.StudyDate);
            if (outputStudy is null || (sourceStudy.Value.Date - outputStudy.Value.Date).Days <= 0)
            {
                errors.Add("StudyDate missing or not shifted");
            }
        }

        // When both are present, they must have moved by the same amount.
        if (sourceBirth is not null && sourceStudy is not null)
        {
            var outputBirth = GetDate(output.Dataset, DicomTags.PatientBirthDate);
            var outputStudy = GetDate(output.Dataset, DicomTags.StudyDate);

            if (outputBirth is not null && outputStudy is not null)
            {
                var birthOffset = (sourceBirth.Value.Date - outputBirth.Value.Date).Days;
                var studyOffset = (sourceStudy.Value.Date - outputStudy.Value.Date).Days;

                if (birthOffset != studyOffset)
                {
                    errors.Add($"Offsets differ: birth={birthOffset}, study={studyOffset}");
                }
            }
        }

        return errors.Count == 0
            ? new ValidationResult(Name, true)
            : new ValidationResult(Name, false, string.Join("; ", errors));
    }

    private static DateTime? GetDate(DicomDataset dataset, DicomTagId tag)
        => dataset.TryGetSingleValue(tag.ToDicomTag(), out DateTime value) ? value : null;
}

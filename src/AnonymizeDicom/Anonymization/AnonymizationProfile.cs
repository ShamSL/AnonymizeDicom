using AnonymizeDicom.Dicom;
using AnonymizeDicom.Anonymization.Strategies;

namespace AnonymizeDicom.Anonymization;

public sealed class AnonymizationProfile
{
    private readonly IReadOnlyDictionary<DicomTagId, IAnonymizationStrategy> _strategies;

    public AnonymizationProfile(IReadOnlyDictionary<DicomTagId, IAnonymizationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IEnumerable<DicomTagId> WhitelistTags => _strategies.Keys;

    // Tables A.1-A.5 from the assignment.
    public static AnonymizationProfile CreateDefault()
    {
        var keep = KeepOriginalStrategy.Instance;
        var shift = DateShiftStrategy.Instance;

        var map = new Dictionary<DicomTagId, IAnonymizationStrategy>
        {
            // For encoding issue.
            [DicomTags.SpecificCharacterSet] = keep,

            // A.1 Patient
            [DicomTags.PatientBirthDate] = shift,
            [DicomTags.PatientSex] = keep,

            // A.2 Study
            [DicomTags.StudyDate] = shift,
            [DicomTags.StudyInstanceUID] = keep,

            // A.3 Series
            [DicomTags.Modality] = keep,

            // A.4 Equipment
            [DicomTags.Manufacturer] = keep,
            [DicomTags.ManufacturerModelName] = keep,
            [DicomTags.SoftwareVersions] = keep,
            [DicomTags.ImagerPixelSpacing] = keep,

            // A.5 Image
            [DicomTags.Rows] = keep,
            [DicomTags.Columns] = keep,
            [DicomTags.PixelSpacing] = keep,
            [DicomTags.BitsAllocated] = keep,
            [DicomTags.PixelRepresentation] = keep,
            [DicomTags.SmallestImagePixelValue] = keep,
            [DicomTags.LargestImagePixelValue] = keep,
            [DicomTags.PixelData] = keep,
            [DicomTags.SOPClassUID] = keep,
            [DicomTags.SOPInstanceUID] = keep,
        };

        return new AnonymizationProfile(map);
    }

    public IAnonymizationStrategy GetStrategy(DicomTagId tag)
        => _strategies.TryGetValue(tag, out var strategy) ? strategy : RemoveStrategy.Instance;

    public bool IsWhitelisted(DicomTagId tag) => _strategies.ContainsKey(tag);
}

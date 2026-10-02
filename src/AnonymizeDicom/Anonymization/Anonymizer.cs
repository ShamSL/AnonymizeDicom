using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization;

public sealed class Anonymizer
{
    private readonly AnonymizationProfile _profile;

    public Anonymizer(AnonymizationProfile profile)
    {
        _profile = profile;
    }

    public void Anonymize(DicomDataset dataset, int offsetDays)
    {
        var context = new AnonymizationContext(offsetDays);

        // strategies will be applied on datasets
        foreach (var item in dataset.ToList())
        {
            var tag = DicomTagId.From(item.Tag);
            _profile.GetStrategy(tag).Apply(dataset, tag, context);
        }
    }
}

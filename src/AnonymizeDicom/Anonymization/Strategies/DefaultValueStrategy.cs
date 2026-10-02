using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization.Strategies;

public sealed class DefaultValueStrategy : IAnonymizationStrategy
{
    private readonly string _defaultValue;

    public DefaultValueStrategy(string defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public void Apply(DicomDataset dataset, DicomTagId tag, AnonymizationContext context)
        => dataset.AddOrUpdate(tag.ToDicomTag(), _defaultValue);
}

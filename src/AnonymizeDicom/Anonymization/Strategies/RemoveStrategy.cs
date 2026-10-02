using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization.Strategies;

public sealed class RemoveStrategy : IAnonymizationStrategy
{
    public static RemoveStrategy Instance { get; } = new();

    private RemoveStrategy()
    {
    }

    public void Apply(DicomDataset dataset, DicomTagId tag, AnonymizationContext context)
    {
        var dicomTag = tag.ToDicomTag();
        if (dataset.Contains(dicomTag))
        {
            dataset.Remove(dicomTag);
        }
    }
}

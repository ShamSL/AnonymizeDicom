using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization.Strategies;

public sealed class KeepOriginalStrategy : IAnonymizationStrategy
{
    public static KeepOriginalStrategy Instance { get; } = new();

    private KeepOriginalStrategy()
    {
    }

    public void Apply(DicomDataset dataset, DicomTagId tag, AnonymizationContext context)
    {
      
    }
}

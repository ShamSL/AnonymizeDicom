using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization.Strategies;

public interface IAnonymizationStrategy
{
    void Apply(DicomDataset dataset, DicomTagId tag, AnonymizationContext context);
}

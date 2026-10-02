using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Anonymization.Strategies;

public sealed class DateShiftStrategy : IAnonymizationStrategy
{
    public static DateShiftStrategy Instance { get; } = new();

    private DateShiftStrategy()
    {
    }

    public void Apply(DicomDataset dataset, DicomTagId tag, AnonymizationContext context)
    {
        if (!dataset.TryGetSingleValue(tag.ToDicomTag(), out DateTime date))
        {
            return;
        }

        dataset.AddOrUpdate(tag.ToDicomTag(), date.AddDays(-context.OffsetDays));
    }
}

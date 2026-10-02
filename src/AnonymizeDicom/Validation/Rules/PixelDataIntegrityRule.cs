using AnonymizeDicom.Dicom;
using FellowOakDicom;

namespace AnonymizeDicom.Validation.Rules;

public sealed class PixelDataIntegrityRule : IValidationRule
{
    public string Name => "PixelDataIntegrity";

    public ValidationResult Validate(DicomFile source, DicomFile output)
    {
        var sourcePixels = GetPixelData(source.Dataset);
        var outputPixels = GetPixelData(output.Dataset);

        if (sourcePixels is null && outputPixels is null)
        {
            return new ValidationResult(Name, true, "No pixel data");
        }

        if (sourcePixels is null || outputPixels is null)
        {
            return new ValidationResult(Name, false, "PixelData presence differs");
        }

        return sourcePixels.AsSpan().SequenceEqual(outputPixels)
            ? new ValidationResult(Name, true)
            : new ValidationResult(Name, false, "Pixel data differs from source");
    }

    private static byte[]? GetPixelData(DicomDataset dataset)
        => dataset.Contains(DicomTag.PixelData)
            ? dataset.GetDicomItem<DicomElement>(DicomTag.PixelData)?.Buffer.Data
            : null;
}

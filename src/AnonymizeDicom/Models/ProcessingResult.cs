namespace AnonymizeDicom.Models;

public class ProcessingResult
{
    public ProcessingSummary Summary { get; init; } = new();
    public IReadOnlyList<FilePair> ProcessedFiles { get; init; } = Array.Empty<FilePair>();
}

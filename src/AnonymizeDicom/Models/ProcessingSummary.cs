namespace AnonymizeDicom.Models;

public class ProcessingSummary
{
    public int TotalFiles { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; } = new();
}

using System.Diagnostics;
using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Logging;
using AnonymizeDicom.Models;
using FellowOakDicom;
using Microsoft.Extensions.Logging;

namespace AnonymizeDicom.Services;

public sealed class AnonymizationService
{
    private readonly Anonymizer _anonymizer;
    private readonly DateOffsetGenerator _offsetGenerator;

    private static readonly ILogger<AnonymizationService> Logger = Log.For<AnonymizationService>();

    public AnonymizationService(Anonymizer anonymizer, DateOffsetGenerator offsetGenerator)
    {
        _anonymizer = anonymizer;
        _offsetGenerator = offsetGenerator;
    }

    public async Task<ProcessingResult> RunAsync(
        string inputFolder,
        string outputFolder,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(inputFolder))
        {
            throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");
        }

        Directory.CreateDirectory(outputFolder);

        var allFiles = Directory.GetFiles(inputFolder, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
        });

        var summary = new ProcessingSummary();

        var processed = new List<FilePair>();
        var successCount = 0;
        var failedCount = 0;
        var skippedCount = 0;
        var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
        var processedBag = new System.Collections.Concurrent.ConcurrentBag<FilePair>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism),
            CancellationToken = cancellationToken,
        };

        var batchStopwatch = Stopwatch.StartNew();

        await Parallel.ForEachAsync(allFiles, parallelOptions, (file, _) =>
        {
            var fileStopwatch = Stopwatch.StartNew();

            try
            {
                var dicomFile = DicomFile.Open(file);
                _anonymizer.Anonymize(dicomFile.Dataset, _offsetGenerator.NextOffset());

                var outputPath = Path.Combine(outputFolder, Path.GetRelativePath(inputFolder, file));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                dicomFile.Save(outputPath);

                Interlocked.Increment(ref successCount);
                processedBag.Add(new FilePair(file, outputPath));

               // Logger.LogInformation("Anonymized {File} in {Ms:F1} ms", Path.GetFileName(file), fileStopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                // Incase of a failure, we want to know if the file was a DICOM file or not.
                if (IsDicom(file))
                {
                    Interlocked.Increment(ref failedCount);
                    errors.Add($"{file}: {ex.Message}");
                    Logger.LogError(ex, "Failed {File} after {Ms:F1} ms", Path.GetFileName(file), fileStopwatch.Elapsed.TotalMilliseconds);
                }
                else
                {
                    Interlocked.Increment(ref skippedCount);
                    Logger.LogInformation("Skipped {File} (not a DICOM file)", Path.GetFileName(file));
                }
            }

            return ValueTask.CompletedTask;
        });

        batchStopwatch.Stop();
        Logger.LogInformation("Processed {Count} files in {Seconds:F2} s", successCount + failedCount, batchStopwatch.Elapsed.TotalSeconds);

        summary.TotalFiles = successCount + failedCount;
        summary.SuccessCount = successCount;
        summary.FailedCount = failedCount;
        summary.SkippedCount = skippedCount;
        summary.Errors.AddRange(errors);
        processed.AddRange(processedBag);

        return new ProcessingResult
        {
            Summary = summary,
            ProcessedFiles = processed,
        };
    }

    private static bool IsDicom(string path)
    {
        try
        {
            return DicomFile.HasValidHeader(path);
        }
        catch
        {
            return false;
        }
    }
}

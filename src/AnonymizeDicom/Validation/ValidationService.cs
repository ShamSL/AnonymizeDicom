using System.Collections.Concurrent;
using AnonymizeDicom.Logging;
using AnonymizeDicom.Models;
using FellowOakDicom;
using Microsoft.Extensions.Logging;

namespace AnonymizeDicom.Validation;

public sealed class ValidationService
{
    private readonly IReadOnlyList<IValidationRule> _rules;

    private static readonly ILogger<ValidationService> Logger = Log.For<ValidationService>();

    public ValidationService(IReadOnlyList<IValidationRule> rules)
    {
        _rules = rules;
    }

    public async Task<ValidationReport> ValidateAsync(
        IReadOnlyList<FilePair> pairs,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        var report = new ValidationReport();
        var results = new ConcurrentBag<ValidationResult>();

        await Parallel.ForEachAsync(pairs, new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism),
            CancellationToken = cancellationToken,
        }, (pair, _) =>
        {
            try
            {
                var source = DicomFile.Open(pair.SourcePath);
                var output = DicomFile.Open(pair.OutputPath);

                foreach (var rule in _rules)
                {
                    results.Add(rule.Validate(source, output));
                }
            }
            catch (Exception ex)
            {
                results.Add(new ValidationResult("Open", false, $"{pair.OutputPath}: {ex.Message}"));
            }

            return ValueTask.CompletedTask;
        });

        foreach (var result in results)
        {
            report.Results.Add(result);

            if (result.Passed)
            {
                report.PassedCount++;
            }
            else
            {
                report.FailedCount++;
            }
        }

        Logger.LogInformation("Validation completed: {Passed} passed, {Failed} failed", report.PassedCount, report.FailedCount);

        foreach (var failed in report.Results.Where(r => !r.Passed))
        {
            Logger.LogError("Validation failed [{Rule}]: {Message}", failed.RuleName, failed.Message);
        }

        return report;
    }
}

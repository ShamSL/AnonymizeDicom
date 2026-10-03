using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Cli;
using AnonymizeDicom.Services;
using AnonymizeDicom.Validation;
using AnonymizeDicom.Validation.Rules;

var options = ArgumentParser.Parse(args);
if (options is null) return 1;

if (!Directory.Exists(options.InputFolder))
{
    Console.Error.WriteLine($"Error: input folder not found: {options.InputFolder}");
    return 1;
}

Directory.CreateDirectory(options.OutputFolder);

// DICOM work is memory-bound; cap concurrency.
var parallelism = Math.Min(4, Math.Max(1, Environment.ProcessorCount));

var profile = AnonymizationProfile.CreateDefault();
var service = new AnonymizationService(
    new Anonymizer(profile),
    new DateOffsetGenerator());

Console.WriteLine($"Anonymizing DICOM files from '{options.InputFolder}' to '{options.OutputFolder}'...");
var result = await service.RunAsync(options.InputFolder, options.OutputFolder, parallelism);

Console.WriteLine();
Console.WriteLine($"Total files: {result.Summary.TotalFiles}");
Console.WriteLine($"Anonymized : {result.Summary.SuccessCount}");
Console.WriteLine($"Failed      : {result.Summary.FailedCount}");
Console.WriteLine($"Skipped    : {result.Summary.SkippedCount} (non-DICOM) ");
foreach (var error in result.Summary.Errors) Console.Error.WriteLine($"  ERROR: {error}");

var validator = new ValidationService(new List<IValidationRule>
{
    new DicomIntegrityRule(),
    new WhitelistComplianceRule(profile),
    new PreservedTagsRule(profile),
    new DateShiftConsistencyRule(),
    new PixelDataIntegrityRule(),
});

var report = await validator.ValidateAsync(result.ProcessedFiles, parallelism);

Console.WriteLine();
Console.WriteLine(report.FailedCount == 0 ? "Validation : SUCCESS" : "Validation : FAILED");

return result.Summary.FailedCount > 0 || report.FailedCount > 0 ? 1 : 0;

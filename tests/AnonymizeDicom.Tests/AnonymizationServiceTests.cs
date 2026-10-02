using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Services;
using AnonymizeDicom.Validation;
using AnonymizeDicom.Validation.Rules;

namespace AnonymizeDicom.Tests;

public class AnonymizationServiceTests
{
    private static AnonymizationService CreateService() =>
        new(
            new Anonymizer(AnonymizationProfile.CreateDefault()),
            new DateOffsetGenerator(1, 1000));

    [Fact]
    public async Task RunAsync_AnonymizesAllFilesAndValidates()
    {
        var inputDir = Path.Combine(Path.GetTempPath(), $"dicom_in_{Guid.NewGuid():N}");
        var outputDir = Path.Combine(Path.GetTempPath(), $"dicom_out_{Guid.NewGuid():N}");
        Directory.CreateDirectory(inputDir);

        try
        {
            DicomTestFactory.Create().Save(Path.Combine(inputDir, "a.dcm"));
            DicomTestFactory.Create().Save(Path.Combine(inputDir, "b.dcm"));
            DicomTestFactory.Create().Save(Path.Combine(inputDir, "c.dcm"));

            var profile = AnonymizationProfile.CreateDefault();
            var service = CreateService();

            var result = await service.RunAsync(inputDir, outputDir, maxDegreeOfParallelism: 4);

            Assert.Equal(3, result.Summary.TotalFiles);
            Assert.Equal(3, result.Summary.SuccessCount);
            Assert.Equal(0, result.Summary.FailedCount);
            Assert.True(File.Exists(Path.Combine(outputDir, "a.dcm")));

            var validator = new ValidationService(new List<IValidationRule>
            {
                new DicomIntegrityRule(),
                new WhitelistComplianceRule(profile),
                new PreservedTagsRule(profile),
                new DateShiftConsistencyRule(),
                new PixelDataIntegrityRule(),
            });

            var report = await validator.ValidateAsync(result.ProcessedFiles, maxDegreeOfParallelism: 4);

            Assert.Equal(0, report.FailedCount);
        }
        finally
        {
            if (Directory.Exists(inputDir))
            {
                Directory.Delete(inputDir, recursive: true);
            }

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RunAsync_ProcessesFilesInSubfolders()
    {
        var inputDir = Path.Combine(Path.GetTempPath(), $"dicom_in_{Guid.NewGuid():N}");
        var outputDir = Path.Combine(Path.GetTempPath(), $"dicom_out_{Guid.NewGuid():N}");
        var nestedDir = Path.Combine(inputDir, "nested");
        Directory.CreateDirectory(nestedDir);

        try
        {
            DicomTestFactory.Create().Save(Path.Combine(inputDir, "top.dcm"));
            DicomTestFactory.Create().Save(Path.Combine(nestedDir, "nested.dcm"));

            var result = await CreateService().RunAsync(inputDir, outputDir, maxDegreeOfParallelism: 4);

            Assert.Equal(2, result.Summary.TotalFiles);
            Assert.Equal(2, result.Summary.SuccessCount);
            Assert.Equal(0, result.Summary.FailedCount);
            Assert.True(File.Exists(Path.Combine(outputDir, "top.dcm")));
            Assert.True(File.Exists(Path.Combine(outputDir, "nested", "nested.dcm")));
        }
        finally
        {
            if (Directory.Exists(inputDir))
            {
                Directory.Delete(inputDir, recursive: true);
            }

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RunAsync_SkipsNonDicomFiles()
    {
        var inputDir = Path.Combine(Path.GetTempPath(), $"dicom_in_{Guid.NewGuid():N}");
        var outputDir = Path.Combine(Path.GetTempPath(), $"dicom_out_{Guid.NewGuid():N}");
        Directory.CreateDirectory(inputDir);

        try
        {
            DicomTestFactory.Create().Save(Path.Combine(inputDir, "a.dcm"));
            File.WriteAllText(Path.Combine(inputDir, "notes.txt"), "not a dicom");
            File.WriteAllBytes(Path.Combine(inputDir, "archive.zip"), new byte[] { 0x50, 0x4B, 0x03, 0x04 });

            var result = await CreateService().RunAsync(inputDir, outputDir, maxDegreeOfParallelism: 4);

            Assert.Equal(1, result.Summary.TotalFiles);
            Assert.Equal(1, result.Summary.SuccessCount);
            Assert.Equal(0, result.Summary.FailedCount);
            Assert.Equal(2, result.Summary.SkippedCount);
        }
        finally
        {
            if (Directory.Exists(inputDir))
            {
                Directory.Delete(inputDir, recursive: true);
            }

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }
}

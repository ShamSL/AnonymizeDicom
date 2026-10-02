using FellowOakDicom;
using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Dicom;
using AnonymizeDicom.Validation;
using AnonymizeDicom.Validation.Rules;

namespace AnonymizeDicom.Tests;

public class ValidationTests
{
    private static IReadOnlyList<IValidationRule> CreateRules(AnonymizationProfile profile) =>
        new List<IValidationRule>
        {
            new DicomIntegrityRule(),
            new WhitelistComplianceRule(profile),
            new PreservedTagsRule(profile),
            new DateShiftConsistencyRule(),
            new PixelDataIntegrityRule(),
        };

    [Fact]
    public void AllRules_PassForAnonymizedFile()
    {
        var profile = AnonymizationProfile.CreateDefault();
        var sourcePath = DicomTestFactory.SaveToTempFile();
        var outputPath = Path.Combine(Path.GetTempPath(), $"dicom_out_{Guid.NewGuid():N}.dcm");

        try
        {
            var source = DicomFile.Open(sourcePath);
            new Anonymizer(profile).Anonymize(source.Dataset, offsetDays: 321);
            source.Save(outputPath);

            var original = DicomFile.Open(sourcePath);
            var anonymized = DicomFile.Open(outputPath);

            foreach (var rule in CreateRules(profile))
            {
                var result = rule.Validate(original, anonymized);
                Assert.True(result.Passed, $"{rule.Name}: {result.Message}");
            }
        }
        finally
        {
            File.Delete(sourcePath);
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void WhitelistCompliance_FailsWhenPatientNameSurvives()
    {
        var profile = AnonymizationProfile.CreateDefault();
        var sourcePath = DicomTestFactory.SaveToTempFile();
        var outputPath = Path.Combine(Path.GetTempPath(), $"dicom_out_{Guid.NewGuid():N}.dcm");

        try
        {
            var source = DicomFile.Open(sourcePath);
            source.Dataset.AddOrUpdate(DicomTags.PatientName.ToDicomTag(), "Doe^Jane"); // simulate a leak
            source.Save(outputPath);

            var original = DicomFile.Open(sourcePath);
            var leaked = DicomFile.Open(outputPath);

            var rule = new WhitelistComplianceRule(profile);
            var result = rule.Validate(original, leaked);

            Assert.False(result.Passed);
        }
        finally
        {
            File.Delete(sourcePath);
            File.Delete(outputPath);
        }
    }
}

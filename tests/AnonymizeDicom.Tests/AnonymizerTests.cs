using FellowOakDicom;
using AnonymizeDicom.Anonymization;
using AnonymizeDicom.Dicom;

namespace AnonymizeDicom.Tests;

public class AnonymizerTests
{
    private readonly Anonymizer _anonymizer = new(AnonymizationProfile.CreateDefault());

    [Fact]
    public void RemovesNonWhitelistedTags()
    {
        var file = DicomTestFactory.Create();

        _anonymizer.Anonymize(file.Dataset, offsetDays: 100);

        Assert.False(file.Dataset.Contains(DicomTags.PatientName.ToDicomTag()));
        Assert.False(file.Dataset.Contains(DicomTags.PatientID.ToDicomTag()));
    }

    [Fact]
    public void KeepsWhitelistedTags()
    {
        var file = DicomTestFactory.Create();

        _anonymizer.Anonymize(file.Dataset, offsetDays: 100);

        Assert.True(file.Dataset.Contains(DicomTags.PatientSex.ToDicomTag()));
        Assert.True(file.Dataset.Contains(DicomTags.StudyInstanceUID.ToDicomTag()));
        Assert.True(file.Dataset.Contains(DicomTags.SOPInstanceUID.ToDicomTag()));
        Assert.True(file.Dataset.Contains(DicomTags.PixelData.ToDicomTag()));
        Assert.Equal("F", file.Dataset.GetString(DicomTags.PatientSex.ToDicomTag()));
    }

    [Fact]
    public void ShiftsDatesBySameOffset()
    {
        var file = DicomTestFactory.Create();
        var sourceBirth = file.Dataset.GetSingleValue<DateTime>(DicomTags.PatientBirthDate.ToDicomTag());
        var sourceStudy = file.Dataset.GetSingleValue<DateTime>(DicomTags.StudyDate.ToDicomTag());

        _anonymizer.Anonymize(file.Dataset, offsetDays: 200);

        var outputBirth = file.Dataset.GetSingleValue<DateTime>(DicomTags.PatientBirthDate.ToDicomTag());
        var outputStudy = file.Dataset.GetSingleValue<DateTime>(DicomTags.StudyDate.ToDicomTag());

        Assert.Equal(200, (sourceBirth.Date - outputBirth.Date).Days);
        Assert.Equal(200, (sourceStudy.Date - outputStudy.Date).Days);
    }

    [Fact]
    public void PreservesPixelData()
    {
        var file = DicomTestFactory.Create();
        var original = file.Dataset.GetDicomItem<DicomElement>(DicomTag.PixelData)!.Buffer.Data;

        _anonymizer.Anonymize(file.Dataset, offsetDays: 100);

        Assert.True(original.AsSpan().SequenceEqual(file.Dataset.GetDicomItem<DicomElement>(DicomTag.PixelData)!.Buffer.Data));
    }

    [Fact]
    public void PreservesNonAsciiTextInKeptFields()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"dicom_cs_{Guid.NewGuid():N}.dcm");
        var outputPath = Path.Combine(Path.GetTempPath(), $"dicom_cs_out_{Guid.NewGuid():N}.dcm");

        try
        {
            var sopUid = DicomUIDGenerator.GenerateDerivedFromUUID();
            var dataset = new DicomDataset();
            dataset.AddOrUpdate(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, sopUid);
            dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, "ISO_IR 100");
            dataset.AddOrUpdate(DicomTag.ManufacturerModelName, "Copyright© Corp");
            dataset.AddOrUpdate(DicomTag.PatientName, "Doe^Jane");
            dataset.AddOrUpdate(DicomTag.Rows, (ushort)2);
            dataset.AddOrUpdate(DicomTag.Columns, (ushort)2);
            dataset.AddOrUpdate(DicomTag.BitsAllocated, (ushort)8);
            dataset.AddOrUpdate(DicomTag.PixelData, new byte[4]);

            var file = new DicomFile(dataset);
            file.FileMetaInfo.MediaStorageSOPClassUID = DicomUID.SecondaryCaptureImageStorage;
            file.FileMetaInfo.MediaStorageSOPInstanceUID = sopUid;
            file.FileMetaInfo.TransferSyntax = DicomTransferSyntax.ExplicitVRLittleEndian;
            file.Save(sourcePath);

            var opened = DicomFile.Open(sourcePath);
            _anonymizer.Anonymize(opened.Dataset, offsetDays: 100);
            opened.Save(outputPath);

            var output = DicomFile.Open(outputPath);

            Assert.False(output.Dataset.Contains(DicomTags.PatientName.ToDicomTag()));
            Assert.Equal("Copyright© Corp", output.Dataset.GetString(DicomTags.ManufacturerModelName.ToDicomTag()));
        }
        finally
        {
            File.Delete(sourcePath);
            File.Delete(outputPath);
        }
    }
}

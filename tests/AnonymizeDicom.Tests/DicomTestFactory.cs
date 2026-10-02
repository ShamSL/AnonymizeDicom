using FellowOakDicom;

namespace AnonymizeDicom.Tests;

/// <summary>
/// Builds synthetic DICOM files for testing, including patient-identifying
/// fields that must be removed by the anonymizer.
/// </summary>
internal static class DicomTestFactory
{
    public static DicomFile Create() => CreateFoDicomFile();

    public static string SaveToTempFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dicom_test_{Guid.NewGuid():N}.dcm");
        CreateFoDicomFile().Save(path);
        return path;
    }

    private static DicomFile CreateFoDicomFile()
    {
        var sopInstanceUid = DicomUIDGenerator.GenerateDerivedFromUUID();

        var dataset = new DicomDataset();

        dataset.AddOrUpdate(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.AddOrUpdate(DicomTag.SOPInstanceUID, sopInstanceUid);

        // Patient-identifiers removed.
        dataset.AddOrUpdate(DicomTag.PatientName, "Doe^Jane");
        dataset.AddOrUpdate(DicomTag.PatientID, "PATIENT-12345");

        // Whitelisted fields (A.1–A.5).
        dataset.AddOrUpdate(DicomTag.PatientBirthDate, new DateTime(1980, 5, 15));
        dataset.AddOrUpdate(DicomTag.PatientSex, "F");
        dataset.AddOrUpdate(DicomTag.StudyDate, new DateTime(2020, 6, 1));
        dataset.AddOrUpdate(DicomTag.StudyInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID());
        dataset.AddOrUpdate(DicomTag.Modality, "OT");
        dataset.AddOrUpdate(DicomTag.Manufacturer, "ACME");
        dataset.AddOrUpdate(DicomTag.ManufacturerModelName, "ModelX");
        dataset.AddOrUpdate(DicomTag.SoftwareVersions, "1.0.0");
        dataset.AddOrUpdate(DicomTag.Rows, (ushort)64);
        dataset.AddOrUpdate(DicomTag.Columns, (ushort)64);
        dataset.AddOrUpdate(DicomTag.BitsAllocated, (ushort)8);
        dataset.AddOrUpdate(DicomTag.PixelRepresentation, (ushort)0);
        dataset.AddOrUpdate(DicomTag.PixelData, new byte[64 * 64]);

        var file = new DicomFile(dataset);
        file.FileMetaInfo.MediaStorageSOPClassUID = DicomUID.SecondaryCaptureImageStorage;
        file.FileMetaInfo.MediaStorageSOPInstanceUID = sopInstanceUid;
        file.FileMetaInfo.TransferSyntax = DicomTransferSyntax.ExplicitVRLittleEndian;

        return file;
    }
}

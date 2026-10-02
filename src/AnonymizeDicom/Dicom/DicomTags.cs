namespace AnonymizeDicom.Dicom;


public static class DicomTags
{
    public static readonly DicomTagId SpecificCharacterSet = new(0x0008, 0x0005);
    public static readonly DicomTagId SOPClassUID = new(0x0008, 0x0016);
    public static readonly DicomTagId SOPInstanceUID = new(0x0008, 0x0018);
    public static readonly DicomTagId StudyDate = new(0x0008, 0x0020);
    public static readonly DicomTagId Modality = new(0x0008, 0x0060);
    public static readonly DicomTagId Manufacturer = new(0x0008, 0x0070);
    public static readonly DicomTagId ManufacturerModelName = new(0x0008, 0x1090);
    public static readonly DicomTagId PatientName = new(0x0010, 0x0010);
    public static readonly DicomTagId PatientID = new(0x0010, 0x0020);
    public static readonly DicomTagId PatientBirthDate = new(0x0010, 0x0030);
    public static readonly DicomTagId PatientSex = new(0x0010, 0x0040);
    public static readonly DicomTagId SoftwareVersions = new(0x0018, 0x1020);
    public static readonly DicomTagId ImagerPixelSpacing = new(0x0018, 0x1164);
    public static readonly DicomTagId StudyInstanceUID = new(0x0020, 0x000d);
    public static readonly DicomTagId Rows = new(0x0028, 0x0010);
    public static readonly DicomTagId Columns = new(0x0028, 0x0011);
    public static readonly DicomTagId PixelSpacing = new(0x0028, 0x0030);
    public static readonly DicomTagId BitsAllocated = new(0x0028, 0x0100);
    public static readonly DicomTagId PixelRepresentation = new(0x0028, 0x0103);
    public static readonly DicomTagId SmallestImagePixelValue = new(0x0028, 0x0106);
    public static readonly DicomTagId LargestImagePixelValue = new(0x0028, 0x0107);
    public static readonly DicomTagId PixelData = new(0x7FE0, 0x0010);
}

using FellowOakDicom;

namespace AnonymizeDicom.Dicom;

// A DICOM tag is just (group, element). Private tags also carry their creator
// string, otherwise two private tags with the same numbers are indistinguishable.
public readonly record struct DicomTagId(ushort Group, ushort Element, string? PrivateCreator = null)
{
    public static DicomTagId From(DicomTag tag)
        => new(tag.Group, tag.Element, tag.PrivateCreator?.Creator);

    public DicomTag ToDicomTag()
    {
        if (PrivateCreator is not null)
        {
            return new DicomTag(Group, Element, new DicomPrivateCreator(PrivateCreator));
        }

        var provisional = new DicomTag(Group, Element);

        // Odd groups are private and not in the standard dictionary.
        if (Group % 2 == 1)
        {
            return provisional;
        }

        return DicomDictionary.Default[provisional]?.Tag ?? provisional;
    }

    public override string ToString() => PrivateCreator is null
        ? $"({Group:X4},{Element:X4})"
        : $"({Group:X4},{Element:X4})[{PrivateCreator}]";
}

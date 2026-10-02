# anonymizeDicom

Command-line tool for anonymizing DICOM files. Point it at a folder and it writes cleaned
copies to another folder, keeping only a fixed set of non-identifying tags and removing
everything else (patient names, IDs, and so on).

## Usage

```
anonymizeDicom --InputFolder C:\DICOM\original --OutputFolder C:\DICOM\anonymized
```

That's the whole interface — just those two options, plus `--help`.

A few things worth knowing:

- PatientBirthDate and StudyDate are shifted by the same random number of days for each image,
  so the dates are wrong but the gap between them still checks out.
- After processing, every output file is re-opened and compared with its source, then the tool
  prints either `Validation : SUCCESS` or `Validation : FAILED`.
- Per-file timings and any failures go to `anonymizeDicom.log`, next to the exe.
- Anything in the input folder that isn't DICOM (zips, text files, whatever) is skipped.

## Building

Needs the .NET 10 SDK.

```
dotnet build AnonymizeDicom.slnx
dotnet test
```

The exe ends up at `src/AnonymizeDicom/bin/Debug/net10.0/anonymizeDicom.exe`.

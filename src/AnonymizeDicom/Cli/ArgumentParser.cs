namespace AnonymizeDicom.Cli;

public static class ArgumentParser
{
    public static CommandLineOptions? Parse(string[] args)
    {
        string? input = null;
        string? output = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--InputFolder":
                    if (!TryRead(args, ref i, out input)) return Error("--InputFolder requires a value.");
                    break;

                case "--OutputFolder":
                    if (!TryRead(args, ref i, out output)) return Error("--OutputFolder requires a value.");
                    break;

                case "-h":
                case "--help":
                    PrintUsage();
                    return null;

                default:
                    return Error($"Unknown argument: {args[i]}");
            }
        }

        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
        {
            PrintUsage();
            return null;
        }

        return new CommandLineOptions
        {
            InputFolder = input,
            OutputFolder = output,
        };
    }

    private static bool TryRead(string[] args, ref int i, out string? value)
    {
        value = null;
        if (i + 1 >= args.Length) return false;
        value = args[++i];
        return true;
    }

    private static CommandLineOptions? Error(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
        PrintUsage();
        return null;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: anonymizeDicom --InputFolder <path> --OutputFolder <path>");
        Console.WriteLine("Options:");
        Console.WriteLine("  --InputFolder <path>            Folder containing input DICOM files (required)");
        Console.WriteLine("  --OutputFolder <path>           Folder for anonymized output files (required)");
    }
}

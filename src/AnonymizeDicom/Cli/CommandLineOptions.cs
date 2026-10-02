namespace AnonymizeDicom.Cli;

public sealed record CommandLineOptions
{
    public string InputFolder { get; init; } = string.Empty;
    public string OutputFolder { get; init; } = string.Empty;
}

using System.CommandLine;

public static class GlobalOptions
{
    public static Option<string> Format = new("--format", () => "json", "Output format (json, csv, gsql)");
    public static Option<string?> FileName = new("--filename", "Dump output to file");
}

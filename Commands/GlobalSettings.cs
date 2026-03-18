using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class GlobalSettings : CommandSettings
{
    [CommandOption("--format|-f")]
    public string? Format { get; set; }
    
    [CommandOption("--filename")]
    public string? Filename { get; set; }
}

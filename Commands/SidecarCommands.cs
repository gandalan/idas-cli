using IdasCli.Sidecars;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class SidecarListCommand : AsyncCommand<SidecarListCommand.Settings>
{
    private readonly ILogger<SidecarListCommand> _logger;

    public SidecarListCommand(ILogger<SidecarListCommand> logger)
    {
        _logger = logger;
    }

    public class Settings : GlobalSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var sidecars = SidecarRegistry.GetAll();

        if (!sidecars.Any())
        {
            AnsiConsole.MarkupLine("[yellow]Keine Sidecars gefunden.[/]");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("Sidecars werden in folgenden Verzeichnissen gesucht:");
            AnsiConsole.MarkupLine("  - Ausfuehrungsverzeichnis");
            AnsiConsole.MarkupLine("  - Aktuelles Arbeitsverzeichnis");
            AnsiConsole.MarkupLine("  - Verzeichnisse in IDAS_SIDECAR_PATH");
            AnsiConsole.MarkupLine("  - ~/.idas/commands");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("Sidecars muessen dem Muster [green]idas-<name>[/] entsprechen.");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Befehl", c => c.NoWrap());
        table.AddColumn("Beschreibung", c => c.Width(60));
        table.AddColumn("Pfad", c => c.Width(40));

        foreach (var sidecar in sidecars.OrderBy(s => s.CommandName))
        {
            table.AddRow(
                $"[green]{sidecar.CommandName}[/]",
                sidecar.Description,
                $"[grey]{sidecar.ExecutablePath}[/]");
        }

        AnsiConsole.Write(table);
        return await Task.FromResult(0);
    }
}

using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class RollenListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public RollenListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            RollenWebRoutinen client = new(authSettings);
            var rollen = await client.GetAllAsync();
            await _outputService.DumpOutputAsync(rollen);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class RollenGetCommand : AsyncCommand<RollenGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public RollenGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<ROLLEGUID>")]
        public string RolleGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.RolleGuid, out var rolleGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            RollenWebRoutinen client = new(authSettings);
            var rollen = await client.GetAllAsync();
            var rolle = rollen.FirstOrDefault(r => r.RolleGuid == rolleGuid);
            await _outputService.DumpOutputAsync(rolle);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class RollenSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public RollenSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var sample = new RolleDTO
            {
                RolleGuid = Guid.NewGuid(),
                Name = "BeispielRolle",
                Beschreibung = "Beschreibung der Rolle",
                Berechtigungen = new List<BerechtigungDTO>
                {
                    new BerechtigungDTO
                    {
                        Code = "Vorgang.Lesen"
                    }
                }.ToArray()
            };
            await _outputService.DumpOutputAsync(sample);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class RollenPutCommand : AsyncCommand<RollenPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public RollenPutCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<FILE>")]
        public string File { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            RollenWebRoutinen client = new(authSettings);
            var rolle = JsonSerializer.Deserialize<RolleDTO>(await File.ReadAllTextAsync(settings.File));
            if (rolle == null)
            {
                Console.WriteLine("Ungültige Rolle-Datei.");
                return 1;
            }
            await client.SaveAsync(rolle);
            Console.WriteLine($"Rolle '{rolle.Name}' wurde gespeichert.");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

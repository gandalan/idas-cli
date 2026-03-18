using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class WertelisteListCommand : AsyncCommand<WertelisteListCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public WertelisteListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--includeAuto")]
        public bool IncludeAuto { get; set; } = false;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            WertelistenWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAllAsync(settings.IncludeAuto));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class WertelisteGetCommand : AsyncCommand<WertelisteGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public WertelisteGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<WERTELISTEGUID>")]
        public string WertelisteGuid { get; set; } = string.Empty;

        [CommandOption("--includeAuto")]
        public bool IncludeAuto { get; set; } = false;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.WertelisteGuid, out var wertelisteGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            WertelistenWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAsync(wertelisteGuid, settings.IncludeAuto));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class WertelistePutCommand : AsyncCommand<WertelistePutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public WertelistePutCommand(IIdasAuthService authService)
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
            WertelistenWebRoutinen client = new(authSettings);
            var werteliste = JsonSerializer.Deserialize<WerteListeDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveAsync(werteliste);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

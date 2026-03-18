using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class KontaktListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public KontaktListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            KontaktWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetKontakteAsync());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class KontaktGetCommand : AsyncCommand<KontaktGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public KontaktGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<KONTAKTGUID>")]
        public string KontaktGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.KontaktGuid, out var kontaktGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            KontaktWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetKontaktAsync(kontaktGuid));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class KontaktPutCommand : AsyncCommand<KontaktPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public KontaktPutCommand(IIdasAuthService authService)
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
            KontaktWebRoutinen client = new(authSettings);
            var kontakt = JsonSerializer.Deserialize<KontaktDTO>(await File.ReadAllTextAsync(settings.File));
            Console.WriteLine(JsonSerializer.Serialize(await client.SaveKontaktAsync(kontakt)));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class KontaktSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public KontaktSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            await _outputService.DumpOutputAsync(new KontaktDTO()
            {
                KontaktGuid = Guid.NewGuid(),
                Firmenname = "Musterfirma",
                ChangedDate = DateTime.UtcNow,
                Personen = [new() { PersonGuid = Guid.NewGuid(), Vorname = "Max", Nachname = "Mustermann" }]
            });
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

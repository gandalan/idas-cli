using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class SerieListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public SerieListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            SerienWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAllSerienAsync());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class SerieGetCommand : AsyncCommand<SerieGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public SerieGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<SERIEGUID>")]
        public string SerieGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.SerieGuid, out var serieGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            SerienWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetSerieAsync(serieGuid));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class SeriePutCommand : AsyncCommand<SeriePutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public SeriePutCommand(IIdasAuthService authService)
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
            SerienWebRoutinen client = new(authSettings);
            var serie = JsonSerializer.Deserialize<SerieDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveSerieAsync(serie);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class SerieSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public SerieSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            await _outputService.DumpOutputAsync(new SerieDTO()
            {
                SerieGuid = Guid.NewGuid(),
                Name = "Beispielserie",
                ChangedDate = DateTime.UtcNow
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

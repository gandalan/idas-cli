using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class VarianteListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VarianteListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            VariantenWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAllAsync());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VarianteGetCommand : AsyncCommand<VarianteGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VarianteGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<GUID>")]
        public string Guid { get; set; } = string.Empty;

        [CommandOption("--includeKonfigs")]
        public bool IncludeKonfigs { get; set; } = false;

        [CommandOption("--includeUIDefs")]
        public bool IncludeUIDefs { get; set; } = false;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!System.Guid.TryParse(settings.Guid, out var guid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            VariantenWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAsync(guid, settings.IncludeUIDefs, settings.IncludeKonfigs));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VariantePutCommand : AsyncCommand<VariantePutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public VariantePutCommand(IIdasAuthService authService)
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
            VariantenWebRoutinen client = new(authSettings);
            var variante = JsonSerializer.Deserialize<VarianteDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveVarianteAsync(variante);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VarianteGuidsCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VarianteGuidsCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            VariantenWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAllGuidsAsync());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

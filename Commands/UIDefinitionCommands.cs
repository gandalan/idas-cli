using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class UIDefinitionListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public UIDefinitionListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            UIWebRoutinen client = new(authSettings);
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

public class UIDefinitionGetCommand : AsyncCommand<UIDefinitionGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public UIDefinitionGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<GUID>")]
        public string Guid { get; set; } = string.Empty;
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
            UIWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAsync(guid));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class UIDefinitionPutCommand : AsyncCommand<UIDefinitionPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public UIDefinitionPutCommand(IIdasAuthService authService)
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
            UIWebRoutinen client = new(authSettings);
            var uiDefinition = JsonSerializer.Deserialize<UIDefinitionDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveUIDefinitionAsync(uiDefinition);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

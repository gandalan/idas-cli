using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class BerechtigungListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public BerechtigungListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            BerechtigungWebRoutinen client = new(authSettings);
            var berechtigungen = await client.GetAllAsync();
            await _outputService.DumpOutputAsync(berechtigungen);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

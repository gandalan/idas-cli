using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class WarengruppeListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public WarengruppeListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            WarenGruppeWebRoutinen client = new(authSettings);
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

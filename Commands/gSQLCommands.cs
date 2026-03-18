using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

namespace IdasCli.Commands;

public class GSQLListCommand : AsyncCommand<GlobalSettings>
{
    public async Task GetList(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task GetBeleg(
        CommonParameters commonParams,
        Guid beleg)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            IBOS1ImportRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.LadeBestellungenAsync());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class GSQLGetCommand : AsyncCommand<GSQLGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public GSQLGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task Reset(
        DateTime since)
    {
        [CommandArgument(0, "<BELEGGUID>")]
        public string BelegGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.BelegGuid, out var belegGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            IBOS1ImportRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetgSQLBelegAsync(belegGuid));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class GSQLResetCommand : AsyncCommand<GSQLResetCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public GSQLResetCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<SINCE>")]
        public DateTime Since { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            IBOS1ImportRoutinen client = new(authSettings);
            await client.ResetBestellungenAsync(settings.Since);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

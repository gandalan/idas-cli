using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class LagerbuchungListCommand : AsyncCommand<LagerbuchungListCommand.Settings>
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime from,
        DateTime till)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task PutBuchung(
        CommonParameters commonParams,
        string file)
    {
        [CommandArgument(0, "<FROM>")]
        public DateTime From { get; set; }

        [CommandArgument(1, "<TILL>")]
        public DateTime Till { get; set; }
    }

    public async Task CreateSample(CommonParameters commonParams)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            LagerbestandWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetLagerhistorieAsync(settings.From, settings.Till));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class LagerbuchungPutCommand : AsyncCommand<LagerbuchungPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public LagerbuchungPutCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
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
            LagerbestandWebRoutinen client = new(authSettings);
            var data = JsonSerializer.Deserialize<LagerbuchungDTO>(await File.ReadAllTextAsync(settings.File));
            await _outputService.DumpOutputAsync(await client.LagerbuchungAsync(data));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class LagerbuchungSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public LagerbuchungSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            await _outputService.DumpOutputAsync(new LagerbuchungDTO());
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

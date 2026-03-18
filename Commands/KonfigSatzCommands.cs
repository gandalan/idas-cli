using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class KonfigSatzListCommand : AsyncCommand<GlobalSettings>
{
    public async Task GetList(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task PutKonfigSatz(
        string file)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            KonfigSatzInfoWebRoutinen client = new(authSettings);
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

public class KonfigSatzPutCommand : AsyncCommand<KonfigSatzPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public KonfigSatzPutCommand(IIdasAuthService authService)
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
            KonfigSatzInfoWebRoutinen client = new(authSettings);
            var konfigSatz = JsonSerializer.Deserialize<KonfigSatzInfoDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveKonfigSatzInfoAsync(konfigSatz);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

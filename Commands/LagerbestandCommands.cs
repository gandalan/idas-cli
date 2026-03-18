using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class LagerbestandListCommand : AsyncCommand<LagerbestandListCommand.Settings>
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime? since)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--since")]
        public DateTime? Since { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            LagerbestandWebRoutinen client = new(authSettings);
            await _outputService.DumpOutputAsync(await client.GetAllAsync(settings.Since));
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

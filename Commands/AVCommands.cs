using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

namespace IdasCli.Commands;

public class AVListCommand : AsyncCommand<AVListCommand.Settings>
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime? since)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task GetPos(
        CommonParameters commonParams,
        string? pos)
    {
        [CommandOption("--since")]
        public DateTime? Since { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            AVWebRoutinen client = new(authSettings);
            var data = await client.GetAllBelegPositionenAVAsync(settings.Since ?? DateTime.Now.AddMonths(-1));
            var reduced = data.Select(avpo => new { avpo.Pcode, avpo.BelegPositionAVGuid, avpo.BelegGuid, avpo.BelegPositionGuid });
            await _outputService.DumpOutputAsync(reduced);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class AVPosCommand : AsyncCommand<AVPosCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public AVPosCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<POSGUID>")]
        public string PosGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            Guid.TryParse(settings.PosGuid, out var guid);

            var authSettings = await _authService.GetSettingsAsync();
            AVWebRoutinen client = new(authSettings);
            if (settings.PosGuid == null && guid == Guid.Empty)
            {
                throw new InvalidOperationException("Please provide pos or pcode");
            }
            var result = guid != Guid.Empty ?
                await client.GetBelegPositionAVByIdAsync(guid) :
                (await client.GetBelegPositionAVByPCodeAsync(settings.PosGuid!)).First();

            await _outputService.DumpOutputAsync(result);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

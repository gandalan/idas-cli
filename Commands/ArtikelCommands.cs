using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class ArtikelListCommand : AsyncCommand<GlobalSettings>
{
    public async Task GetList(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task PutArtikel(
        string file)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            ArtikelWebRoutinen client = new(authSettings);
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

public class ArtikelPutCommand : AsyncCommand<ArtikelPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public ArtikelPutCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public async Task CreateSample(CommonParameters commonParams)
    {
        [CommandArgument(0, "<FILE>")]
        public string File { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            ArtikelWebRoutinen client = new(authSettings);
            var artikel = JsonSerializer.Deserialize<KatalogArtikelDTO>(await File.ReadAllTextAsync(settings.File));
            await client.SaveArtikelAsync(artikel);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class ArtikelSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public ArtikelSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            await _outputService.DumpOutputAsync(new KatalogArtikelDTO()
            {
                Art = "KatalogArtikel",
                Bezeichnung = "Testartikel",
                Einheit = "Stk.",
                Preis = 1.99m,
                KatalogArtikelGuid = Guid.NewGuid(),
                KatalogNummer = "99099",
                Freigabe_IBOS = true,
                GueltigAb = DateTime.Now,
                GueltigBis = DateTime.Now.AddYears(1)
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

using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class VorgangListCommand : AsyncCommand<VorgangListCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--jahr")]
        public int? Jahr { get; set; }

        [CommandOption("--includeArchive")]
        public bool IncludeArchive { get; set; } = true;

        [CommandOption("--includeOthersData")]
        public bool IncludeOthersData { get; set; } = true;

        [CommandOption("--includeASP")]
        public bool IncludeASP { get; set; } = true;

        [CommandOption("--includeAdditionalProperties")]
        public bool IncludeAdditionalProperties { get; set; } = true;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            VorgangListeWebRoutinen client = new(authSettings);
            var year = settings.Jahr ?? 0;
            var activeList = await client.LadeVorgangsListeAsync(year, "Alle", DateTime.MinValue, "",
                settings.IncludeArchive, settings.IncludeOthersData, "", settings.IncludeASP, settings.IncludeAdditionalProperties);
            await _outputService.DumpOutputAsync(activeList);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VorgangGetCommand : AsyncCommand<VorgangGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<VORGANGGUID>")]
        public string VorgangGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.VorgangGuid, out var vorgangGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            VorgangWebRoutinen client = new(authSettings);
            var response = await client.LadeVorgangAsync(vorgangGuid, true);
            await _outputService.DumpOutputAsync(response);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VorgangPutCommand : AsyncCommand<VorgangPutCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangPutCommand(IIdasAuthService authService, IOutputService outputService)
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
            VorgangWebRoutinen client = new(authSettings);
            var vorgang = JsonSerializer.Deserialize<VorgangDTO>(await File.ReadAllTextAsync(settings.File));
            if (vorgang != null)
            {
                vorgang.IstZustimmungErteilt = true;
                var response = await client.SendeVorgangAsync(vorgang);
                await _outputService.DumpOutputAsync(response);
                return 0;
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid JSON file[/]");
                return 1;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VorgangSampleCommand : AsyncCommand<GlobalSettings>
{
    private readonly IOutputService _outputService;

    public VorgangSampleCommand(IOutputService outputService)
    {
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var posGuid = Guid.NewGuid();
            await _outputService.DumpOutputAsync(new VorgangDTO()
            {
                IstTestbeleg = true,
                ErstellDatum = DateTime.Now,
                VorgangGuid = Guid.NewGuid(),
                VorgangsNummer = 99099,
                Kommission = "Testvorgang",

                Belege = [
                    new() {
                        Positionen = new List<Guid>() { posGuid },
                        BelegGuid = Guid.NewGuid(),
                        BelegNummer = 99099,
                        BelegArt = "Angebot",
                        BelegDatum = DateTime.Now,
                        BelegJahr = DateTime.Now.Year,
                        InterneNotiz = "Testbeleg!",
                        BelegAdresse = new BeleganschriftDTO() {
                            AdressGuid = Guid.NewGuid(),
                            Anrede = "Herr",
                            Vorname = "Test",
                            Nachname = "Test",
                            Strasse = "Test",
                            Postleitzahl = "12345",
                            Ort = "Test",
                            Land = "DE"
                        },
                        VersandAdresseGleichBelegAdresse = true
                    }
                ],

                Positionen = [
                    new BelegPositionDTO() {
                        BelegPositionGuid = posGuid,
                        Einbauort = "Test",
                        PositionsKommission = "PositionsKommission",
                        ErfassungsDatum = DateTime.Now,
                        Einzelpreis = 1.0m,
                        Menge = 1.0m,
                        IstAktiv = true,
                        ArtikelNummer = "102302.ZS",
                        Daten = [
                            new() { BelegPositionDatenGuid = Guid.NewGuid(), DatenTyp = "string", KonfigName="Besonderheiten", Wert = "Test" }
                        ]
                    }
                ]
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

public class VorgangArchiveCommand : AsyncCommand<VorgangArchiveCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangArchiveCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<VORGANGGUID>")]
        public string VorgangGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.VorgangGuid, out var vorgangGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            VorgangWebRoutinen client = new(authSettings);
            await client.ArchiviereVorgangAsync(vorgangGuid);
            await _outputService.DumpOutputAsync(new { Status = "Archiviert" });
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VorgangArchiveBulkCommand : AsyncCommand<VorgangArchiveBulkCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangArchiveBulkCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<VORGAENGE>")]
        public string Vorgaenge { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            VorgangWebRoutinen client = new(authSettings);

            var guids = settings.Vorgaenge.Split(',')
                .Select(g => Guid.TryParse(g.Trim(), out Guid guid) ? guid : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            if (!guids.Any())
            {
                await _outputService.DumpOutputAsync(new { Status = "Fehler: Keine gültigen GUIDs gefunden" });
                return 1;
            }

            await client.ArchiviereVorgangListAsync(guids);
            await _outputService.DumpOutputAsync(new { Status = "Archiviert", Anzahl = guids.Count });
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class VorgangActivateCommand : AsyncCommand<VorgangActivateCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public VorgangActivateCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<VORGANGGUID>")]
        public string VorgangGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.VorgangGuid, out var vorgangGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            VorgangWebRoutinen client = new(authSettings);
            await client.ArchivierungAufhebenAsync(vorgangGuid);
            await _outputService.DumpOutputAsync(new { Status = "Aktiviert" });
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

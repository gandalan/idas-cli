using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class VorgangListCommand : AsyncCommand<VorgangListCommand.Settings>
{
    public async Task GetList(
        CommonParameters commonParams,
        int? jahr = null,
        bool includeArchive = true,
        bool includeOthersData = true,
        bool includeASP = true,
        bool includeAdditionalProperties = true)
    {
        var settings = await getSettings();
        VorgangListeWebRoutinen client = new(settings);
        var year = jahr ?? 0; // 0 = all years, like the client does

    public VorgangListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task GetVorgang(
        CommonParameters commonParams,
        Guid vorgang)
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

    public async Task PutVorgang(
        CommonParameters commonParams,
        string file)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        var vorgang = JsonSerializer.Deserialize<VorgangDTO>(await File.ReadAllTextAsync(file));
        if (vorgang != null)
        {
            vorgang.IstZustimmungErteilt = true;
            var response = await client.SendeVorgangAsync(vorgang);
            await dumpOutput(commonParams, response);
        } else throw new Exception("Invalid JSON file");
    }

    public async Task CreateSample(CommonParameters commonParams)
    {
        var posGuid = Guid.NewGuid();
        await dumpOutput(commonParams, new VorgangDTO()
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
    }

    public async Task ArchiveVorgang(
        CommonParameters commonParams,
        Guid vorgang)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        await client.ArchiviereVorgangAsync(vorgang);
        await dumpOutput(commonParams, new { Status = "Archiviert" });
    }

    public async Task ArchiveVorgangBulk(
        CommonParameters commonParams,
        string vorgaenge)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);

        var guids = vorgaenge.Split(',')
            .Select(g => Guid.TryParse(g.Trim(), out Guid guid) ? guid : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        if (!guids.Any())
        {
            await dumpOutput(commonParams, new { Status = "Fehler: Keine gültigen GUIDs gefunden" });
            return;
        }

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

    public async Task ActivateVorgang(
        CommonParameters commonParams,
        Guid vorgang)
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
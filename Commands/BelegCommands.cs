using System.Globalization;
using IdasCli.Services;
using System.Text;
using System.Text.Json;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

public class BelegListCommand : AsyncCommand<BelegListCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly ILogger<BelegListCommand> _logger;

    public BelegListCommand(IIdasAuthService authService, ILogger<BelegListCommand> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--jahr")]
        public int Jahr { get; set; } = 0;

        [CommandOption("--format")]
        public new string Format { get; set; } = "json";

        [CommandOption("--separator")]
        public string Separator { get; set; } = ";";

        [CommandOption("--belegart")]
        public string? Belegart { get; set; }

        [CommandOption("--filename")]
        public new string? Filename { get; set; }

        [CommandOption("--includeArchive")]
        public bool IncludeArchive { get; set; } = true;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();

            _logger.LogInformation($"Lade Belege{(settings.Jahr > 0 ? $" für Jahr {settings.Jahr}" : "")}...");

            // 1. Alle Vorgänge laden
            var vorgaengeList = await getAllVorgaenge(settings.Jahr, settings.IncludeArchive, authSettings);
            if (vorgaengeList == null || !vorgaengeList.Any())
            {
                _logger.LogInformation("Keine Vorgänge gefunden.");
                return 0;
            }

            _logger.LogInformation($"{vorgaengeList.Count} Vorgänge gefunden. Lade Details...");

            // 2. Belege extrahieren
            var belege = await extractBelege(vorgaengeList, settings.Belegart, authSettings);

            _logger.LogInformation($"{belege.Count} Belege extrahiert.");

            // 3. Output generieren
            string output;
            if (settings.Format.ToLower() == "csv")
            {
                output = GenerateCsv(belege, settings.Separator);
            }
            else
            {
                output = GenerateJson(belege);
            }

            // 4. Output schreiben
            if (!string.IsNullOrEmpty(settings.Filename))
            {
                await File.WriteAllTextAsync(settings.Filename, output, Encoding.UTF8);
                _logger.LogInformation($"Ergebnis gespeichert in: {settings.Filename}");
            }
            else
            {
                Console.WriteLine(output);
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private async Task<List<VorgangListItemDTO>> getAllVorgaenge(int jahr, bool includeArchive, IWebApiConfig authSettings)
    {
        try
        {
            var client = new VorgangListeWebRoutinen(authSettings);
            var result = await client.LadeVorgangsListeAsync(jahr, "Alle", DateTime.MinValue, "",
                includeArchive, true, "", true, true);
            return result?.ToList() ?? new List<VorgangListItemDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Fehler beim Laden der Vorgänge: {ex.Message}");
            return new List<VorgangListItemDTO>();
        }
    }

    private async Task<List<BelegListDTO>> extractBelege(List<VorgangListItemDTO> vorgaengeList, string? belegartFilter, IWebApiConfig authSettings)
    {
        var belege = new List<BelegListDTO>();
        var total = vorgaengeList.Count;
        var current = 0;

        foreach (var vorgangSummary in vorgaengeList)
        {
            current++;
            try
            {
                // Vorgang Details laden
                var client = new VorgangWebRoutinen(authSettings);
                var vorgang = await client.LadeVorgangAsync(vorgangSummary.VorgangGuid, true);
                if (vorgang?.Belege == null) continue;

                foreach (var beleg in vorgang.Belege)
                {
                    // Belegart filtern
                    if (!string.IsNullOrEmpty(belegartFilter) &&
                        !beleg.BelegArt.Equals(belegartFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var dto = MapBelegToDTO(vorgangSummary, vorgang, beleg);
                    belege.Add(dto);
                }

                if (current % 10 == 0 || current == total)
                {
                    _logger.LogInformation($"  [{current}/{total}] Vorgänge verarbeitet...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"  Fehler bei Vorgang {vorgangSummary.VorgangsNummer}: {ex.Message}");
            }
        }

        return belege;
    }

    private BelegListDTO MapBelegToDTO(VorgangListItemDTO vorgangSummary, VorgangDTO vorgang, BelegDTO beleg)
    {
        var dto = new BelegListDTO
        {
            VorgangsNummer = (int)vorgangSummary.VorgangsNummer,
            Kundenname = vorgangSummary.Kundenname ?? string.Empty,
            KundenNummer = vorgangSummary.KundenNummer ?? string.Empty,
            BelegArt = beleg.BelegArt,
            BelegNummer = (int)beleg.BelegNummer,
            BelegJahr = (int)beleg.BelegJahr,
            BelegDatum = beleg.BelegDatum,
            AnzahlPositionen = beleg.Positionen?.Count ?? 0,
            VorgangGuid = vorgangSummary.VorgangGuid,
            BelegGuid = beleg.BelegGuid,
            IstArchiviert = vorgangSummary.IsArchiv
        };

        // Salden extrahieren
        if (beleg.Salden != null)
        {
            foreach (var saldo in beleg.Salden)
            {
                var betrag = saldo.Betrag;

                switch (saldo.Name)
                {
                    case "Warenwert":
                        dto.Warenwert = betrag;
                        break;
                    case "StandardSaldo":
                        dto.RabatteAufschlaege += betrag; // Summe aller StandardSalden
                        break;
                    case "TransportkostenFirmenkunde":
                        dto.Transportkosten = betrag;
                        break;
                    case "Mehrwertsteuer":
                        dto.Mehrwertsteuer = betrag;
                        break;
                    case "Endbetrag":
                        dto.EndbetragBrutto = betrag;
                        break;
                    case "GesamtbetragNetto":
                        dto.GesamtbetragNetto = betrag;
                        break;
                }
            }
        }

        return dto;
    }

    private string GenerateJson(List<BelegListDTO> belege)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(belege, options);
    }

    private string GenerateCsv(List<BelegListDTO> belege, string separator)
    {
        var sb = new StringBuilder();

        // Header
        var headers = new[]
        {
            "VorgangsNummer", "Kundenname", "KundenNummer", "BelegArt", "BelegNummer",
            "BelegJahr", "BelegDatum", "AnzahlPositionen", "Warenwert", "RabatteAufschlaege",
            "Transportkosten", "Mehrwertsteuer", "EndbetragBrutto", "GesamtbetragNetto",
            "VorgangGuid", "BelegGuid", "IstArchiviert"
        };
        sb.AppendLine(string.Join(separator, headers));

        // Daten
        var culture = new CultureInfo("de-DE");
        foreach (var b in belege)
        {
            var line = new[]
            {
                b.VorgangsNummer.ToString(),
                EscapeCsvField(b.Kundenname, separator),
                EscapeCsvField(b.KundenNummer, separator),
                b.BelegArt,
                b.BelegNummer.ToString(),
                b.BelegJahr.ToString(),
                b.BelegDatum.ToString("dd.MM.yyyy", culture),
                b.AnzahlPositionen.ToString(),
                b.Warenwert.ToString("N2", culture),
                b.RabatteAufschlaege.ToString("N2", culture),
                b.Transportkosten.ToString("N2", culture),
                b.Mehrwertsteuer.ToString("N2", culture),
                b.EndbetragBrutto.ToString("N2", culture),
                b.GesamtbetragNetto.ToString("N2", culture),
                b.VorgangGuid.ToString(),
                b.BelegGuid.ToString(),
                b.IstArchiviert ? "Ja" : "Nein"
            };
            sb.AppendLine(string.Join(separator, line));
        }

        return sb.ToString();
    }

    private string EscapeCsvField(string field, string separator)
    {
        // Wenn das Feld den Separator oder Anführungszeichen enthält, in Anführungszeichen setzen
        if (field.Contains(separator) || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        return field;
    }
}

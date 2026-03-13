using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class VorgangCommands : CommandsBase
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

        // Load Vorgänge with same parameters as the client (using VorgangListeWebRoutinen)
        var activeList = await client.LadeVorgangsListeAsync(year, "Alle", DateTime.MinValue, "",
            includeArchive, includeOthersData, "", includeASP, includeAdditionalProperties);
        await dumpOutput(commonParams, activeList);
    }

    public async Task GetVorgang(
        CommonParameters commonParams,
        Guid vorgang)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        var response = await client.LadeVorgangAsync(vorgang, true);
        await dumpOutput(commonParams, response);
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
            await client.ArchiviereVorgangListAsync(guids);
            await dumpOutput(commonParams, new {
                Status = "Archiviert",
                Anzahl = guids.Count
            });
        }
        catch (Exception ex)
        {
            await dumpOutput(commonParams, new { Status = $"Fehler: {ex.Message}" });
        }
    }

    public async Task ActivateVorgang(
        CommonParameters commonParams,
        Guid vorgang)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        await client.ArchivierungAufhebenAsync(vorgang);
        await dumpOutput(commonParams, new { Status = "Aktiviert" });
    }
}

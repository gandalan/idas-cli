using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class VorgangCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(CommonParameters commonParams,
        [Option("jahr", Description = "Year to list")] int? jahr = null)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        var response = await client.LadeVorgangsListeAsync(jahr ?? DateTime.Now.Year);
        await dumpOutput(commonParams, response);
    }

    [Command("get")]
    public async Task GetVorgang(
        CommonParameters commonParams,
        [Argument("vorgang", Description = "Vorgang-GUID")]
        Guid vorgang)
    {
        var settings = await getSettings();
        VorgangWebRoutinen client = new(settings);
        var response = await client.LadeVorgangAsync(vorgang, true);
        await dumpOutput(commonParams, response);
    }

    [Command("put")]
    public async Task PutVorgang(
        CommonParameters commonParams,
        [Argument("file", Description = "JSON file with Vorgang data")]
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

    [Command("sample", Description = "Create a sample VorgangDTO")]
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
}
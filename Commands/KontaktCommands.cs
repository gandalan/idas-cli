using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class KontaktCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontakteAsync());
    }

    [Command("get")]
    public async Task GetKontakt(
        CommonParameters commonParams,
        [Argument("kontakt", Description = "Kontakt-GUID")]
        Guid kontakt)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontaktAsync(kontakt));
    }

    [Command("put")]
    public async Task PutKontakt(
        [Argument("file", Description = "JSON file with Kontakt data")]
        string file)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        var kontakt = JsonSerializer.Deserialize<KontaktDTO>(await File.ReadAllTextAsync(file));
        Console.WriteLine(JsonSerializer.Serialize(await client.SaveKontaktAsync(kontakt)));
    }

    [Command("sample", Description = "Create a sample KontaktDTO")]
    public async Task CreateSample(CommonParameters commonParams) => await dumpOutput(commonParams, new KontaktDTO()
    {
        KontaktGuid = Guid.NewGuid(),
        Firmenname = "Musterfirma",
        ChangedDate = DateTime.UtcNow,
        Personen = [ new() { PersonGuid = Guid.NewGuid(), Vorname = "Max", Nachname = "Mustermann" } ]
    });

    [Command("serie-list")]
    public async Task GetSerienList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetSerienAsync());
    }

    [Command("serie-get")]
    public async Task GetSerie(
        CommonParameters commonParams,
        [Argument("serie", Description = "Serie-GUID")]
        Guid serie)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetSerieAsync(serie));
    }

    [Command("serie-put")]
    public async Task PutSerie(
        [Argument("file", Description = "JSON file with Serie data")]
        string file)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        var serie = JsonSerializer.Deserialize<SerieDTO>(await File.ReadAllTextAsync(file));
        Console.WriteLine(JsonSerializer.Serialize(await client.SaveSerieAsync(serie)));
    }

    [Command("serie-sample", Description = "Create a sample SerieDTO")]
    public async Task CreateSampleSerie(CommonParameters commonParams) => await dumpOutput(commonParams, new SerieDTO()
    {
        SerieGuid = Guid.NewGuid(),
        Name = "Beispielserie",
        ChangedDate = DateTime.UtcNow
    });
}

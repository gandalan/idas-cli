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
        Firmenname = "Musterfirma",
        Personen = [ new() { Vorname = "Max", Nachname = "Mustermann" } ]
    });
}
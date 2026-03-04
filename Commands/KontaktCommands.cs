using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class KontaktCommands : CommandsBase
{
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontakteAsync());
    }

    public async Task GetKontakt(
        CommonParameters commonParams,
        Guid kontakt)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontaktAsync(kontakt));
    }

    public async Task PutKontakt(
        CommonParameters commonParams,
        string file)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        var kontakt = JsonSerializer.Deserialize<KontaktDTO>(await File.ReadAllTextAsync(file));
        Console.WriteLine(JsonSerializer.Serialize(await client.SaveKontaktAsync(kontakt)));
    }

    public async Task CreateSample(CommonParameters commonParams) => await dumpOutput(commonParams, new KontaktDTO()
    {
        KontaktGuid = Guid.NewGuid(),
        Firmenname = "Musterfirma",
        ChangedDate = DateTime.UtcNow,
        Personen = [ new() { PersonGuid = Guid.NewGuid(), Vorname = "Max", Nachname = "Mustermann" } ]
    });

}

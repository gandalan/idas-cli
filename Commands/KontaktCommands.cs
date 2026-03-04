using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class KontaktCommands : CommandsBase
{
    [CliCommand("list", Description = "List all contacts")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontakteAsync());
    }

    [CliCommand("get", Description = "Get a single contact by GUID")]
    public async Task GetKontakt(
        CommonParameters commonParams,
        [CliArgument(Description = "Contact GUID")] Guid kontakt)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetKontaktAsync(kontakt));
    }

    [CliCommand("put", Description = "Update a contact from JSON file")]
    public async Task PutKontakt(
        CommonParameters commonParams,
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        KontaktWebRoutinen client = new(settings);
        var kontakt = JsonSerializer.Deserialize<KontaktDTO>(await File.ReadAllTextAsync(file));
        Console.WriteLine(JsonSerializer.Serialize(await client.SaveKontaktAsync(kontakt)));
    }

    [CliCommand("sample", Description = "Create a sample contact JSON")]
    public async Task CreateSample(CommonParameters commonParams) => await dumpOutput(commonParams, new KontaktDTO()
    {
        KontaktGuid = Guid.NewGuid(),
        Firmenname = "Musterfirma",
        ChangedDate = DateTime.UtcNow,
        Personen = [ new() { PersonGuid = Guid.NewGuid(), Vorname = "Max", Nachname = "Mustermann" } ]
    });

}

using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class SerieCommands : CommandsBase
{
    [Command("list")]
    public async Task GetSerienList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllSerienAsync());
    }

    [Command("get")]
    public async Task GetSerie(
        CommonParameters commonParams,
        [Argument("serie", Description = "Serie-GUID")]
        Guid serie)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetSerieAsync(serie));
    }

    [Command("put")]
    public async Task PutSerie(
        [Argument("file", Description = "JSON file with Serie data")]
        string file)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        var serie = JsonSerializer.Deserialize<SerieDTO>(await File.ReadAllTextAsync(file));
        //Console.WriteLine(JsonSerializer.Serialize(await client.SaveSerieAsync(serie)));
        await client.SaveSerieAsync(serie);
    }

    [Command("sample", Description = "Create a sample SerieDTO")]
    public async Task CreateSampleSerie(CommonParameters commonParams) => await dumpOutput(commonParams, new SerieDTO()
    {
        SerieGuid = Guid.NewGuid(),
        Name = "Beispielserie",
        ChangedDate = DateTime.UtcNow
    });
}

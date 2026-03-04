using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class SerieCommands : CommandsBase
{
    public async Task GetSerienList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllSerienAsync());
    }

    public async Task GetSerie(
        CommonParameters commonParams,
        Guid serie)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetSerieAsync(serie));
    }

    public async Task PutSerie(
        string file)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        var serie = JsonSerializer.Deserialize<SerieDTO>(await File.ReadAllTextAsync(file));
        //Console.WriteLine(JsonSerializer.Serialize(await client.SaveSerieAsync(serie)));
        await client.SaveSerieAsync(serie);
    }

    public async Task CreateSampleSerie(CommonParameters commonParams) => await dumpOutput(commonParams, new SerieDTO()
    {
        SerieGuid = Guid.NewGuid(),
        Name = "Beispielserie",
        ChangedDate = DateTime.UtcNow
    });
}

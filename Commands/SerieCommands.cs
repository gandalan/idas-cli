using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class SerieCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Serien")]
    public async Task GetSerienList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllSerienAsync());
    }

    [CliCommand("get", Description = "Get a Serie by GUID")]
    public async Task GetSerie(
        CommonParameters commonParams,
        [CliArgument(Description = "Serie GUID")] Guid serie)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetSerieAsync(serie));
    }

    [CliCommand("put", Description = "Update a Serie from JSON file")]
    public async Task PutSerie(
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        SerienWebRoutinen client = new(settings);
        var serie = JsonSerializer.Deserialize<SerieDTO>(await File.ReadAllTextAsync(file));
        //Console.WriteLine(JsonSerializer.Serialize(await client.SaveSerieAsync(serie)));
        await client.SaveSerieAsync(serie);
    }

    [CliCommand("sample", Description = "Create a sample Serie JSON")]
    public async Task CreateSampleSerie(CommonParameters commonParams) => await dumpOutput(commonParams, new SerieDTO()
    {
        SerieGuid = Guid.NewGuid(),
        Name = "Beispielserie",
        ChangedDate = DateTime.UtcNow
    });
}

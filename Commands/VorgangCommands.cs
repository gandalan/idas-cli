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
        var response = await client.SendeVorgangAsync(vorgang);
        await dumpOutput(commonParams, response);
    }
}
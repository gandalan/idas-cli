using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class KonfigSatzCommands : CommandsBase
{
    [Command("list", Description = "Get all configuration sets")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [Command("put")]
    public async Task PutKonfigSatz(
        [Argument("file", Description = "JSON file with KonfigSatzInfo data")]
        string file)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        var konfigSatz = JsonSerializer.Deserialize<KonfigSatzInfoDTO>(await File.ReadAllTextAsync(file));
        await client.SaveKonfigSatzInfoAsync(konfigSatz);
    }
}

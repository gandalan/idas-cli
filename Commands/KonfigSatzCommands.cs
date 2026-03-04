using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class KonfigSatzCommands : CommandsBase
{
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    public async Task PutKonfigSatz(
        string file)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        var konfigSatz = JsonSerializer.Deserialize<KonfigSatzInfoDTO>(await File.ReadAllTextAsync(file));
        await client.SaveKonfigSatzInfoAsync(konfigSatz);
    }
}

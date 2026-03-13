using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class WertelisteCommands : CommandsBase
{
    public async Task GetList(
        CommonParameters commonParams,
        bool includeAuto)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(includeAuto));
    }

    public async Task GetWerteliste(
        Guid guid,
        CommonParameters commonParams,
        bool includeAuto)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeAuto));
    }

    public async Task PutWerteliste(
        string file)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        var werteliste = JsonSerializer.Deserialize<WerteListeDTO>(await File.ReadAllTextAsync(file));
        await client.SaveAsync(werteliste);
    }
}

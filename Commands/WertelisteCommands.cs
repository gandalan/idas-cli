using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class WertelisteCommands : CommandsBase
{
    [Command("list", Description = "Get all value lists")]
    public async Task GetList(
        CommonParameters commonParams,
        [Option("include-auto", Description = "Include auto-generated value lists")] bool includeAuto = true)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(includeAuto));
    }

    [Command("get")]
    public async Task GetWerteliste(
        [Argument("guid", Description = "Werteliste GUID")]
        Guid guid,
        CommonParameters commonParams,
        [Option("include-auto", Description = "Include auto-generated value lists")] bool includeAuto = true)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeAuto));
    }

    [Command("put")]
    public async Task PutWerteliste(
        [Argument("file", Description = "JSON file with Werteliste data")]
        string file)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        var werteliste = JsonSerializer.Deserialize<WerteListeDTO>(await File.ReadAllTextAsync(file));
        await client.SaveAsync(werteliste);
    }
}

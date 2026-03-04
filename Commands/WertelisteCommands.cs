using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class WertelisteCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Wertelisten")]
    public async Task GetList(
        CommonParameters commonParams,
        [CliOption(Description = "Include automatically generated entries")] bool includeAuto)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(includeAuto));
    }

    [CliCommand("get", Description = "Get a Werteliste by GUID")]
    public async Task GetWerteliste(
        [CliArgument(Description = "Werteliste GUID")] Guid guid,
        CommonParameters commonParams,
        [CliOption(Description = "Include automatically generated entries")] bool includeAuto)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeAuto));
    }

    [CliCommand("put", Description = "Update a Werteliste from JSON file")]
    public async Task PutWerteliste(
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        WertelistenWebRoutinen client = new(settings);
        var werteliste = JsonSerializer.Deserialize<WerteListeDTO>(await File.ReadAllTextAsync(file));
        await client.SaveAsync(werteliste);
    }
}

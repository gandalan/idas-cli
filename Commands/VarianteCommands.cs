using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class VarianteCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Varianten")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [CliCommand("get", Description = "Get a Variante by GUID")]
    public async Task GetVariante(
        CommonParameters commonParams,
        [CliArgument(Description = "Variante GUID")] Guid guid,
        [CliOption(Description = "Include Konfiguration data")] bool includeKonfigs,
        [CliOption(Description = "Include UI Definitions")] bool includeUIDefs)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeUIDefs, includeKonfigs));
    }

    [CliCommand("put", Description = "Update a Variante from JSON file")]
    public async Task PutVariante(
        CommonParameters commonParams,
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        var variante = JsonSerializer.Deserialize<VarianteDTO>(await File.ReadAllTextAsync(file));
        await client.SaveVarianteAsync(variante);
    }

    [CliCommand("all-guids", Description = "Get all Variante GUIDs")]
    public async Task GetAllGuids(CommonParameters commonParams)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllGuidsAsync());
    }
}

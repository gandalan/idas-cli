using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class VarianteCommands : CommandsBase
{
    [Command("list", Description = "Get all variants")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [Command("get")]
    public async Task GetVariante(
        [Argument("guid", Description = "Variante GUID")]
        Guid guid,
        CommonParameters commonParams,
        [Option("include-uidefs", Description = "Include UI definitions")] bool includeUIDefs = true,
        [Option("include-konfigs", Description = "Include configurations")] bool includeKonfigs = true)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeUIDefs, includeKonfigs));
    }

    [Command("put")]
    public async Task PutVariante(
        [Argument("file", Description = "JSON file with Variante data")]
        string file)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        var variante = JsonSerializer.Deserialize<VarianteDTO>(await File.ReadAllTextAsync(file));
        await client.SaveVarianteAsync(variante);
    }

    [Command("guids")]
    public async Task GetAllGuids(CommonParameters commonParams)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllGuidsAsync());
    }
}

using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class UIDefinitionCommands : CommandsBase
{
    [CliCommand("list", Description = "List all UI Definitions")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [CliCommand("get", Description = "Get a UI Definition by GUID")]
    public async Task GetUIDefinition(
        CommonParameters commonParams,
        [CliArgument(Description = "UI Definition GUID")] Guid guid)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid));
    }

    [CliCommand("put", Description = "Update a UI Definition from JSON file")]
    public async Task PutUIDefinition(
        CommonParameters commonParams,
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        var uiDefinition = JsonSerializer.Deserialize<UIDefinitionDTO>(await File.ReadAllTextAsync(file));
        await client.SaveUIDefinitionAsync(uiDefinition);
    }
}

using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class UIDefinitionCommands : CommandsBase
{
    [Command("list", Description = "Get all UI definitions")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [Command("get")]
    public async Task GetUIDefinition(
        [Argument("guid", Description = "UIDefinition GUID")]
        Guid guid,
        CommonParameters commonParams)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid));
    }

    [Command("put")]
    public async Task PutUIDefinition(
        [Argument("file", Description = "JSON file with UIDefinition data")]
        string file)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        var uiDefinition = JsonSerializer.Deserialize<UIDefinitionDTO>(await File.ReadAllTextAsync(file));
        await client.SaveUIDefinitionAsync(uiDefinition);
    }
}

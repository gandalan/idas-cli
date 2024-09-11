using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class LagerbestandCommands : CommandsBase
{
    [Command("list", Description = "Get the inventory list")]
    public async Task GetList(CommonParameters commonParams, 
        [Option("since", Description = "Reset since")] DateTime? since)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(since));
    }
}
using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class WarengruppeCommands : CommandsBase
{
    [Command("list", Description = "Get the list of product groups, including all their products")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        WarenGruppeWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }
}
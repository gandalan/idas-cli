using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class WarengruppeCommands : CommandsBase
{
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        WarenGruppeWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }
}
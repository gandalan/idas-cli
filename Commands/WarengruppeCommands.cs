using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class WarengruppeCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Warengruppen")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        WarenGruppeWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }
}
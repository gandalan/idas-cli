using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class LagerbestandCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Lagerbestand items")]
    public async Task GetList(
        CommonParameters commonParams,
        [CliOption(Description = "Only items changed since this date")] DateTime? since)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(since));
    }
}
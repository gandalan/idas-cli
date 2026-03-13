using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class LagerbestandCommands : CommandsBase
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime? since)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync(since));
    }
}
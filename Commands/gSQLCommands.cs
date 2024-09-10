using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class gSQLCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.LadeBestellungenAsync());
    }

    [Command("get")]
    public async Task GetBeleg(
        CommonParameters commonParams,
        [Argument("beleg", Description = "Beleg-GUID")]
        Guid beleg)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        var paramsOverride = new CommonParameters { Format = "gsql", FileName = commonParams.FileName };
        await dumpOutput(paramsOverride, await client.GetgSQLBelegAsync(beleg));
    }

    [Command("reset")]
    public async Task Reset(
        [Argument("since", Description = "Reset since")]
        DateTime since)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await client.ResetBestellungenAsync(since);
    }
}
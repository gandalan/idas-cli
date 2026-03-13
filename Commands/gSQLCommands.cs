using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class gSQLCommands : CommandsBase
{
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.LadeBestellungenAsync());
    }

    public async Task GetBeleg(
        CommonParameters commonParams,
        Guid beleg)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        var paramsOverride = new CommonParameters { Format = "gsql", FileName = commonParams.FileName };
        await dumpOutput(paramsOverride, await client.GetgSQLBelegAsync(beleg));
    }

    public async Task Reset(
        DateTime since)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await client.ResetBestellungenAsync(since);
    }
}